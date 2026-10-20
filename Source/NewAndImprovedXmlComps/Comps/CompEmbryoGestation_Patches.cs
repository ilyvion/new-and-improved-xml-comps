using System.Reflection.Emit;

namespace NewAndImprovedXmlComps;

[HarmonyPatch(typeof(Building_GrowthVat))]
public partial class CompEmbryoGestation
{
    internal static IEnumerable<CodeInstruction> EmbryoGestationTicksTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        string patchedMethodName,
        CodeInstruction[]? getBuildingInstructions = null
    )
    {
        var codeMatcher = new CodeMatcher(instructions, generator);

        _ = codeMatcher.SearchForward(i =>
            i.opcode == OpCodes.Ldc_I4
            && i.operand is int value
            && value == Building_GrowthVat.EmbryoGestationTicks
        );
        if (!codeMatcher.IsValid)
        {
            Log.Error(
                $"Could not patch {patchedMethodName}, IL does not match expectations: ldc.i4 for Building_GrowthVat.EmbryoGestationTicks not found."
            );
            return codeMatcher.Instructions();
        }

#pragma warning disable IDE0047
        _ = codeMatcher
            .RemoveInstruction()
            .Insert([
                .. (getBuildingInstructions ?? [new(OpCodes.Ldarg_0)]),
                new(
                    OpCodes.Call,
                    AccessTools.Method(typeof(CompEmbryoGestation), nameof(GetEmbryoGestationTicks))
                ),
            ]);
#pragma warning restore IDE0047

        return codeMatcher.Instructions();
    }

    [HarmonyPatch(nameof(Building_GrowthVat.DrawAt))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> DrawAtTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    ) =>
        EmbryoGestationTicksTranspiler(
            instructions,
            generator,
            $"{nameof(Building_GrowthVat)}.{nameof(Building_GrowthVat.DrawAt)}"
        );

    [HarmonyPatch(nameof(Building_GrowthVat.TryGrowEmbryo))]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> TryGrowEmbryoTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    ) =>
        EmbryoGestationTicksTranspiler(
            instructions,
            generator,
            $"{nameof(Building_GrowthVat)}.{nameof(Building_GrowthVat.TryGrowEmbryo)}"
        );

    [HarmonyPatch(nameof(Building_GrowthVat.EmbryoGestationPct), MethodType.Getter)]
    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> EmbryoGestationPctTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        var codeMatcher = new CodeMatcher(instructions, generator);

        _ = codeMatcher.SearchForward(i =>
            i.opcode == OpCodes.Ldc_R4
            && i.operand is float value
            && value == Building_GrowthVat.EmbryoGestationTicks
        );
        if (!codeMatcher.IsValid)
        {
            Log.Error(
                $"Could not patch {nameof(Building_GrowthVat)}.{nameof(Building_GrowthVat.EmbryoGestationPct)}, IL does not match expectations: ldc.r4 for Building_GrowthVat.EmbryoGestationTicks not found."
            );
            return codeMatcher.Instructions();
        }

        _ = codeMatcher
            .RemoveInstruction()
            .Insert([
                new(OpCodes.Ldarg_0),
                new(
                    OpCodes.Call,
                    AccessTools.Method(typeof(CompEmbryoGestation), nameof(GetEmbryoGestationTicks))
                ),
                new(OpCodes.Conv_R4),
            ]);

        return codeMatcher.Instructions();
    }

    // vat is null when HumanEmbryo.GetGizmos finds no available growth vat.
    internal static int GetEmbryoGestationTicks(Building_GrowthVat? vat) =>
        vat?.GetComp<CompEmbryoGestation>()?.PropsEmbryoGestation.embryoGestationTicks
        ?? Building_GrowthVat.EmbryoGestationTicks;
}

[HarmonyPatch]
internal sealed class Building_GrowthVat_GetGizmos_Patch
{
    private static MethodInfo GetGizmos =>
        AccessTools.Method(typeof(Building_GrowthVat), nameof(Building_GrowthVat.GetGizmos));

    internal static MethodInfo TargetMethod() => AccessTools.EnumeratorMoveNext(GetGizmos);

    // GetGizmos itself only creates the iterator and stores `this` in it with ldarg.0 + stfld.
    internal static FieldInfo? IteratorThisField()
    {
        var iteratorType = TargetMethod().DeclaringType;
        var instructions = PatchProcessor.GetOriginalInstructions(GetGizmos);
        for (var i = 1; i < instructions.Count; i++)
        {
            if (
                instructions[i - 1].opcode == OpCodes.Ldarg_0
                && instructions[i].opcode == OpCodes.Stfld
                && instructions[i].operand is FieldInfo field
                && field.DeclaringType == iteratorType
            )
            {
                return field;
            }
        }
        return null;
    }

    internal static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        var thisField = IteratorThisField();
        if (thisField == null)
        {
            Log.Error(
                "Could not patch Building_GrowthVat.GetGizmos, IL does not match expectations: store of this into the iterator not found."
            );
            return instructions;
        }

        return CompEmbryoGestation.EmbryoGestationTicksTranspiler(
            instructions,
            generator,
            $"{nameof(Building_GrowthVat)}.{nameof(Building_GrowthVat.GetGizmos)}",
            [new(OpCodes.Ldarg_0), new(OpCodes.Ldfld, thisField)]
        );
    }
}

[HarmonyPatch]
internal sealed class HumanEmbryo_GetGizmos_Patch
{
    internal static MethodInfo TargetMethod() =>
        AccessTools.EnumeratorMoveNext(
            AccessTools.Method(typeof(HumanEmbryo), nameof(HumanEmbryo.GetGizmos))
        );

    internal static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        var codeMatcher = new CodeMatcher(instructions, generator);
        _ = codeMatcher.SearchForward(i =>
            i.Calls(
                AccessTools.Method(typeof(HumanEmbryo), nameof(HumanEmbryo.BestAvailableGrowthVat))
            )
        );
        if (!codeMatcher.IsValid)
        {
            Log.Error(
                "Could not patch HumanEmbryo.GetGizmos, IL does not match expectations: call to BestAvailableGrowthVat not found."
            );
            return codeMatcher.Instructions();
        }

        // Keep a copy of the vat BestAvailableGrowthVat returns, for the gestation ticks lookup.
        var bestVat = generator.DeclareLocal(typeof(Building_GrowthVat));
        _ = codeMatcher.Advance(1).Insert([new(OpCodes.Dup), new(OpCodes.Stloc, bestVat)]);

        return CompEmbryoGestation.EmbryoGestationTicksTranspiler(
            codeMatcher.Instructions(),
            generator,
            $"{nameof(HumanEmbryo)}.{nameof(HumanEmbryo.GetGizmos)}",
            [new(OpCodes.Ldloc, bestVat)]
        );
    }
}

[HarmonyPatch(typeof(HumanEmbryo), nameof(HumanEmbryo.BestAvailableGrowthVat))]
internal sealed class HumanEmbryo_BestAvailableGrowthVat_Patch
{
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    internal static IEnumerable<CodeInstruction> Transpiler(
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        var originalInstructions = instructions.ToList();
        var codeMatcher = new CodeMatcher(instructions, generator);

        _ = codeMatcher.SearchForward(i => i.CallMatches(m => m.Name == "SortBy"));
        if (!codeMatcher.IsValid)
        {
            Log.Error(
                "Could not patch HumanEmbryo.BestAvailableGrowthVat, IL does not match expectations: call to SortBy method not found."
            );
            return originalInstructions;
        }

        _ = codeMatcher.Advance(-3);
        if (codeMatcher.Instruction.opcode != OpCodes.Ldarg_0)
        {
            Log.Error(
                "Could not patch HumanEmbryo.BestAvailableGrowthVat, IL does not match expectations: expected Ldarg_0 before SortBy call; was "
                    + codeMatcher.Instruction
            );
            return originalInstructions;
        }

        var labels = codeMatcher.Labels.ToList();
        codeMatcher.Labels.Clear();

        _ = codeMatcher.Insert([
            new(
                OpCodes.Call,
                AccessTools.Method(
                    typeof(HumanEmbryo_BestAvailableGrowthVat_Patch),
                    nameof(SortByEmbryoGestationTicks)
                )
            )
            {
                labels = labels,
            },
        ]);

        // Since we're adding another sort criterion, we need to change the SortBy call to use the correct overload.
        // The original code uses a generic SortBy method with 3 type parameters, so we need to change it to use the 4-parameter overload.
        _ = codeMatcher.SearchForward(i => i.CallMatches(m => m.Name == "SortBy"));
        if (!codeMatcher.IsValid)
        {
            Log.Error(
                "Could not patch HumanEmbryo.BestAvailableGrowthVat, IL does not match expectations: call to SortBy method not found."
            );
            return originalInstructions;
        }
        codeMatcher.Instruction.operand = typeof(GenCollection)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "SortBy" && m.GetGenericArguments().Length == 4)
            .MakeGenericMethod(typeof(Building_GrowthVat), typeof(int), typeof(int), typeof(float));

        return codeMatcher.Instructions();
    }

    internal static Func<Building_GrowthVat, int> SortByEmbryoGestationTicks() =>
        vat =>
        {
            var comp = vat.GetComp<CompEmbryoGestation>();
            var embryoGestationTicks =
                comp?.PropsEmbryoGestation.embryoGestationTicks
                ?? Building_GrowthVat.EmbryoGestationTicks;
            return embryoGestationTicks;
        };
}

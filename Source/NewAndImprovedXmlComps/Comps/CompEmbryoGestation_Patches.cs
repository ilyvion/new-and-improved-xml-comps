using System.Reflection.Emit;

namespace NewAndImprovedXmlComps;

[HarmonyPatch(typeof(Building_GrowthVat))]
public partial class CompEmbryoGestation
{
    internal static IEnumerable<CodeInstruction> EmbryoGestationTicksTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
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
                "Could not reverse patch DrawAt, IL does not match expectations: ldc.i4 for Building_GrowthVat.EmbryoGestationTicks not found."
            );
            return codeMatcher.Instructions();
        }

#pragma warning disable IDE0047
        _ = codeMatcher
            .RemoveInstruction()
            .Insert(
                [
                    .. (getBuildingInstructions ?? [new(OpCodes.Ldarg_0)]),
                    new(
                        OpCodes.Call,
                        AccessTools.Method(
                            typeof(CompEmbryoGestation),
                            nameof(GetEmbryoGestationTicks)
                        )
                    ),
                ]
            );
#pragma warning restore IDE0047

        return codeMatcher.Instructions();
    }

    [HarmonyPatch(nameof(Building_GrowthVat.DrawAt))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> DrawAtTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    ) => EmbryoGestationTicksTranspiler(instructions, generator);

    [HarmonyPatch(nameof(Building_GrowthVat.TryGrowEmbryo))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> TryGrowEmbryoTranspiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    ) => EmbryoGestationTicksTranspiler(instructions, generator);

    private static int GetEmbryoGestationTicks(Building_GrowthVat vat) =>
        vat.GetComp<CompEmbryoGestation>()?.PropsEmbryoGestation.embryoGestationTicks
        ?? Building_GrowthVat.EmbryoGestationTicks;
}

[HarmonyPatch]
internal sealed class Building_GrowthVat_GetGizmos_Patch
{
    private static MethodInfo TargetMethod()
    {
        var closures = AccessTools.FirstInner(
            typeof(Building_GrowthVat),
            t => t.Name.Contains("<GetGizmos>"
#if !v1_5
                    , StringComparison.Ordinal
#endif
                )
        );
        return AccessTools.Method(closures, nameof(IEnumerator.MoveNext));
    }

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        var codeMatcher = new CodeMatcher(instructions, generator);
        _ = codeMatcher.SearchForward(i =>
            i.opcode == OpCodes.Stfld
            && i.operand is FieldInfo f
            && f.Name.Contains("__this"
#if !v1_5
                , StringComparison.Ordinal
#endif
            )
        );
        if (!codeMatcher.IsValid)
        {
            Log.Error(
                "Could not patch Building_GrowthVat.GetGizmos, IL does not match expectations: access to __this field not found."
            );
            return codeMatcher.Instructions();
        }
        var instruction = codeMatcher.Instruction;

        return CompEmbryoGestation.EmbryoGestationTicksTranspiler(
            instructions,
            generator,
            [new(OpCodes.Ldarg_0), new(OpCodes.Ldfld, instruction.operand)]
        );
    }
}

[HarmonyPatch]
internal sealed class HumanEmbryo_GetGizmos_Patch
{
    private static MethodInfo TargetMethod()
    {
        var closures = AccessTools.FirstInner(
            typeof(HumanEmbryo),
            t => t.Name.Contains("<GetGizmos>"
#if !v1_5
                    , StringComparison.Ordinal
#endif
                )
        );
        return AccessTools.Method(closures, nameof(IEnumerator.MoveNext));
    }

    private static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    )
    {
        var codeMatcher = new CodeMatcher(instructions, generator);
        _ = codeMatcher.SearchForward(i =>
            i.opcode == OpCodes.Stfld && i.operand is FieldInfo f && f.Name == "bestVat"
        );
        if (!codeMatcher.IsValid)
        {
            Log.Error(
                "Could not patch HumanEmbryo.GetGizmos, IL does not match expectations: access to bestVat field not found."
            );
            return codeMatcher.Instructions();
        }
        var bestVatInstruction = codeMatcher.Instruction;

        _ = codeMatcher.Advance(-3);
        if (codeMatcher.Instruction.opcode != OpCodes.Ldloc_S)
        {
            Log.Error(
                "Could not patch HumanEmbryo.GetGizmos, IL does not match expectations: expected Ldloc_S before Stfld bestVat; was "
                    + codeMatcher.Instruction
            );
            return codeMatcher.Instructions();
        }
        var bestVatLocalInstruction = codeMatcher.Instruction;

        return CompEmbryoGestation.EmbryoGestationTicksTranspiler(
            instructions,
            generator,
            [
                new(OpCodes.Ldloc_S, bestVatLocalInstruction.operand),
                new(OpCodes.Ldfld, bestVatInstruction.operand),
            ]
        );
    }
}

[HarmonyPatch(typeof(HumanEmbryo), nameof(HumanEmbryo.BestAvailableGrowthVat))]
internal sealed class HumanEmbryo_BestAvailableGrowthVat_Patch
{
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private static IEnumerable<CodeInstruction> Transpiler(
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
                "Could not patch HumanEmbryo.GetGizmos, IL does not match expectations: call to SortBy method not found."
            );
            return originalInstructions;
        }

        _ = codeMatcher.Advance(-3);
        if (codeMatcher.Instruction.opcode != OpCodes.Ldarg_0)
        {
            Log.Error(
                "Could not patch HumanEmbryo.GetGizmos, IL does not match expectations: expected Ldarg_0 before SortBy call; was "
                    + codeMatcher.Instruction
            );
            return originalInstructions;
        }

        var labels = codeMatcher.Labels.ToList();
        codeMatcher.Labels.Clear();

        _ = codeMatcher.Insert(
            [
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
            ]
        );

        // Since we're adding another sort criterion, we need to change the SortBy call to use the correct overload.
        // The original code uses a generic SortBy method with 3 type parameters, so we need to change it to use the 4-parameter overload.
        _ = codeMatcher.SearchForward(i => i.CallMatches(m => m.Name == "SortBy"));
        if (!codeMatcher.IsValid)
        {
            Log.Error(
                "Could not patch HumanEmbryo.GetGizmos, IL does not match expectations: call to SortBy method not found."
            );
            return originalInstructions;
        }
        codeMatcher.Instruction.operand = typeof(GenCollection)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "SortBy" && m.GetGenericArguments().Length == 4)
            .MakeGenericMethod(typeof(Building_GrowthVat), typeof(int), typeof(int), typeof(float));

        return codeMatcher.Instructions();
    }

    private static Func<Building_GrowthVat, int> SortByEmbryoGestationTicks() =>
        vat =>
        {
            var comp = vat.GetComp<CompEmbryoGestation>();
            var embryoGestationTicks =
                comp?.PropsEmbryoGestation.embryoGestationTicks
                ?? Building_GrowthVat.EmbryoGestationTicks;
            return embryoGestationTicks;
        };
}

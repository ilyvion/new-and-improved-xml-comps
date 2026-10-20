using System.Reflection.Emit;
using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

// Each transpiler logs an error and leaves the IL unchanged when vanilla's IL no longer matches,
// which happens at startup, before any test runs. Re-running it here against the current game's
// IL turns such a mismatch into a failing test.
[TestFixture(TestType.MainMenu)]
internal sealed class TranspilerSmokeTests
{
    private delegate IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator
    );

    private static string HarmonyId => NewAndImprovedXmlCompsMod.Instance.Content.Name;

    private static List<CodeInstruction> Transpile(MethodBase original, Transpiler transpiler) =>
        [
            .. transpiler(
                PatchProcessor.GetOriginalInstructions(original, out var generator),
                generator
            ),
        ];

    private static void ExpectPatchedWith(MethodBase original, Transpiler transpiler) =>
        Expect.Any(
            (IEnumerable<Patch>?)Harmony.GetPatchInfo(original)?.Transpilers ?? [],
            p => p.owner == HarmonyId && p.PatchMethod == transpiler.Method
        );

    private static void ExpectCalls(List<CodeInstruction> instructions, MethodInfo hook) =>
        Expect.Any(instructions, i => i.Calls(hook), $"call to {hook.Name}");

    private static void ExpectNoVanillaGestationTicks(List<CodeInstruction> instructions) =>
        Expect.None(
            instructions,
            i =>
                i.opcode == OpCodes.Ldc_I4
                && i.operand is int value
                && value == Building_GrowthVat.EmbryoGestationTicks
        );

    private static readonly MethodInfo IsActive = AccessTools.Method(
        typeof(CompImprovedSpawner),
        nameof(CompImprovedSpawner.IsActive),
        Type.EmptyTypes
    );

    private static readonly MethodInfo GetEmbryoGestationTicks = AccessTools.Method(
        typeof(CompEmbryoGestation),
        nameof(CompEmbryoGestation.GetEmbryoGestationTicks)
    );

    private static CompImprovedSpawner MakeUnspawnedSpawner() =>
        new()
        {
            parent = new ThingWithComps(),
            props = new CompProperties_ImprovedSpawner { writeTimeLeftToSpawn = false },
        };

    // An unapplied reverse patch runs the stub, which logs the transpiler's error.
    [Test]
    public static void SpawnerTickReversePatchIsApplied() => MakeUnspawnedSpawner().CompTick();

    [Test]
    public static void SpawnerTickTranspilerInsertsIsActive() =>
        ExpectCalls(
            Transpile(
                AccessTools.Method(typeof(CompSpawner), nameof(CompSpawner.TickIntervalDelta)),
                CompImprovedSpawner.TickIntervalDeltaTranspiler
            ),
            IsActive
        );

    [Test]
    public static void SpawnerInspectStringReversePatchIsApplied() =>
        Expect.IsNull(MakeUnspawnedSpawner().CompInspectStringExtra());

    [Test]
    public static void SpawnerInspectStringTranspilerInsertsIsActive() =>
        ExpectCalls(
            Transpile(
                AccessTools.Method(typeof(CompSpawner), nameof(CompSpawner.CompInspectStringExtra)),
                CompImprovedSpawner.CompInspectStringExtraTranspiler
            ),
            IsActive
        );

    [Test]
    public static void PowerGridOverlayIsPatched()
    {
        var original = AccessTools.Method(
            typeof(CompPower),
            nameof(CompPower.CompPrintForPowerGrid)
        );
        ExpectPatchedWith(original, CompImprovedPower_CompPrintForPowerGrid.CompPrintForPowerGrid);
        ExpectCalls(
            Transpile(original, CompImprovedPower_CompPrintForPowerGrid.CompPrintForPowerGrid),
            AccessTools.Method(
                typeof(CompProperties_ImprovedPower),
                nameof(CompProperties_ImprovedPower.ShouldPrintWirePieceConnecting)
            )
        );
    }

    [Test]
    public static void PowerPostPrintOntoIsPatched()
    {
        var original = AccessTools.Method(typeof(CompPower), nameof(CompPower.PostPrintOnto));
        ExpectPatchedWith(original, CompImprovedPower_CompPrintForPowerGrid.PostPrintOnto);
        ExpectCalls(
            Transpile(original, CompImprovedPower_CompPrintForPowerGrid.PostPrintOnto),
            AccessTools.Method(
                typeof(CompProperties_ImprovedPower),
                nameof(CompProperties_ImprovedPower.ShouldPostPrintOnto)
            )
        );
    }

    private static void ExpectGestationTicksPatched(MethodBase original, Transpiler transpiler)
    {
        ExpectPatchedWith(original, transpiler);
        var instructions = Transpile(original, transpiler);
        ExpectCalls(instructions, GetEmbryoGestationTicks);
        ExpectNoVanillaGestationTicks(instructions);
    }

    [Test]
    public static void GrowthVatDrawAtIsPatched() =>
        ExpectGestationTicksPatched(
            AccessTools.Method(typeof(Building_GrowthVat), nameof(Building_GrowthVat.DrawAt)),
            CompEmbryoGestation.DrawAtTranspiler
        );

    [Test]
    public static void GrowthVatEmbryoGestationPctIsPatched()
    {
        var original = AccessTools.PropertyGetter(
            typeof(Building_GrowthVat),
            nameof(Building_GrowthVat.EmbryoGestationPct)
        );
        ExpectPatchedWith(original, CompEmbryoGestation.EmbryoGestationPctTranspiler);
        var instructions = Transpile(original, CompEmbryoGestation.EmbryoGestationPctTranspiler);
        ExpectCalls(instructions, GetEmbryoGestationTicks);
        Expect.None(
            instructions,
            i =>
                i.opcode == OpCodes.Ldc_R4
                && i.operand is float value
                && value == Building_GrowthVat.EmbryoGestationTicks
        );
    }

    [Test]
    public static void GrowthVatTryGrowEmbryoIsPatched() =>
        ExpectGestationTicksPatched(
            AccessTools.Method(
                typeof(Building_GrowthVat),
                nameof(Building_GrowthVat.TryGrowEmbryo)
            ),
            CompEmbryoGestation.TryGrowEmbryoTranspiler
        );

    [Test]
    public static void GrowthVatGetGizmosIsPatched() =>
        ExpectGestationTicksPatched(
            Building_GrowthVat_GetGizmos_Patch.TargetMethod(),
            Building_GrowthVat_GetGizmos_Patch.Transpiler
        );

    // MoveNext's ldarg.0 is the iterator, so the vat must come from a field the iterator declares.
    [Test]
    public static void GrowthVatGetGizmosLoadsTheVatFromTheIterator()
    {
        var original = Building_GrowthVat_GetGizmos_Patch.TargetMethod();
        var instructions = Transpile(original, Building_GrowthVat_GetGizmos_Patch.Transpiler);
        var call = instructions.FindIndex(i => i.Calls(GetEmbryoGestationTicks));
        Expect.IsTrue(call >= 2, "call to GetEmbryoGestationTicks after the vat load");
        if (call < 2)
        {
            return;
        }

        var field = instructions[call - 1].operand as FieldInfo;
        Expect.AreEqual(OpCodes.Ldarg_0, instructions[call - 2].opcode);
        Expect.AreEqual(OpCodes.Ldfld, instructions[call - 1].opcode);
        Expect.AreEqual(original.DeclaringType, field?.DeclaringType);
        Expect.AreEqual(typeof(Building_GrowthVat), field?.FieldType);
    }

    [Test]
    public static void HumanEmbryoGetGizmosIsPatched() =>
        ExpectGestationTicksPatched(
            HumanEmbryo_GetGizmos_Patch.TargetMethod(),
            HumanEmbryo_GetGizmos_Patch.Transpiler
        );

    [Test]
    public static void HumanEmbryoBestAvailableGrowthVatIsPatched()
    {
        var original = AccessTools.Method(
            typeof(HumanEmbryo),
            nameof(HumanEmbryo.BestAvailableGrowthVat)
        );
        ExpectPatchedWith(original, HumanEmbryo_BestAvailableGrowthVat_Patch.Transpiler);
        ExpectCalls(
            Transpile(original, HumanEmbryo_BestAvailableGrowthVat_Patch.Transpiler),
            AccessTools.Method(
                typeof(HumanEmbryo_BestAvailableGrowthVat_Patch),
                nameof(HumanEmbryo_BestAvailableGrowthVat_Patch.SortByEmbryoGestationTicks)
            )
        );
    }

    [Test]
    public static void GravEngineDrawAtIsPatched()
    {
        var original = AccessTools.Method(
            typeof(Building_GravEngine),
            nameof(Building_GravEngine.DrawAt)
        );
        ExpectPatchedWith(original, Building_GravEngine_DrawAt_Patch.Transpiler);
        var instructions = Transpile(original, Building_GravEngine_DrawAt_Patch.Transpiler);
        ExpectCalls(
            instructions,
            AccessTools.Method(
                typeof(Building_GravEngine_DrawAt_Patch),
                nameof(Building_GravEngine_DrawAt_Patch.UpdateVector3)
            )
        );
        ExpectCalls(
            instructions,
            AccessTools.Method(
                typeof(Building_GravEngine_DrawAt_Patch),
                nameof(Building_GravEngine_DrawAt_Patch.GetOrbGraphic)
            )
        );
    }
}

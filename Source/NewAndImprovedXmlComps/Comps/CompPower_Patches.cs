using System.Reflection;
using System.Reflection.Emit;

namespace NewAndImprovedXmlComps;

[HarmonyPatch(typeof(CompPower))]
public partial class CompImprovedPower_CompPrintForPowerGrid
{
    private static readonly FieldInfo ThingDefOf_HiddenConduit
        = AccessTools.Field(typeof(ThingDefOf), nameof(ThingDefOf.HiddenConduit));
    private static readonly FieldInfo CompPower_connectParent
        = AccessTools.Field(typeof(CompPower), nameof(CompPower.connectParent));
    private static readonly MethodInfo CompImprovedPower_ShouldPrintWirePieceConnecting
        = AccessTools.Method(
            typeof(CompProperties_ImprovedPower),
            nameof(CompProperties_ImprovedPower.ShouldPrintWirePieceConnecting));

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CompPower.CompPrintForPowerGrid))]
    private static IEnumerable<CodeInstruction> CompPrintForPowerGrid(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        return PatchWithShould(
            instructions,
            generator,
            nameof(CompPower.CompPrintForPowerGrid),
            CompImprovedPower_ShouldPrintWirePieceConnecting);
    }

    private static readonly MethodInfo CompImprovedPower_ShouldPostPrintOnto
        = AccessTools.Method(
            typeof(CompProperties_ImprovedPower),
            nameof(CompProperties_ImprovedPower.ShouldPostPrintOnto));

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CompPower.PostPrintOnto))]
    private static IEnumerable<CodeInstruction> PostPrintOnto(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        return PatchWithShould(
            instructions,
            generator,
            nameof(CompPower.PostPrintOnto),
            CompImprovedPower_ShouldPostPrintOnto);
    }

    private static IEnumerable<CodeInstruction> PatchWithShould(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator generator,
        string methodName,
        MethodInfo shouldMethodOperand)
    {

        var codeMatcher = new CodeMatcher(instructions, generator);

        var is15 =
#if v1_5
            true;
#else
            false;
#endif
        if (is15 || methodName == nameof(CompPower.PostPrintOnto))
        {
            codeMatcher.SearchForward(i => i.opcode == OpCodes.Ldsfld && i.operand is FieldInfo f && f == ThingDefOf_HiddenConduit);
            if (!codeMatcher.IsValid)
            {
                Log.Error($"Could not patch {methodName}, IL does not match expectations: access to ThingDefOf.HiddenConduit not found.");
                return codeMatcher.Instructions();
            }
        }
        else // v1_6 && (methodName == nameof(CompPower.CompPrintForPowerGrid))
        {
            codeMatcher.End();
            codeMatcher.SearchBackwards(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f == CompPower_connectParent);
            if (!codeMatcher.IsValid)
            {
                Log.Error($"Could not patch {methodName}, IL does not match expectations: access to CompPower.connectParent not found.");
                return codeMatcher.Instructions();
            }
        }

        codeMatcher.SearchBackwards(i => i.opcode == OpCodes.Ldarg_0);
        codeMatcher.Advance(-1);
        codeMatcher.SearchBackwards(i => i.opcode == OpCodes.Ldarg_0);
        codeMatcher.Advance(1);
        if (!codeMatcher.IsValid || codeMatcher.Opcode != OpCodes.Ldfld || codeMatcher.Operand is not FieldInfo f || f != CompPower_connectParent)
        {
            Log.Error($"Could not patch {methodName}, IL does not match expectations: access to CompPower.connectParent not found.");
            return codeMatcher.Instructions();
        }
        codeMatcher.Advance(1);
        var skipLabel = codeMatcher.Operand;

        codeMatcher.Advance(-2);
        var ifLabels = codeMatcher.Labels;
        codeMatcher.Labels = [];

        codeMatcher.Insert([
            new(OpCodes.Ldarg_0) { labels = ifLabels },
            new(OpCodes.Call, shouldMethodOperand),
            new(OpCodes.Brfalse_S, skipLabel),
        ]);

        return codeMatcher.Instructions();
    }
}

using System.Reflection;
using System.Reflection.Emit;

namespace NewAndImprovedXmlComps;

[HarmonyPatch(typeof(CompSpawner))]
public partial class CompImprovedSpawner
{
    private static readonly MethodInfo CompSpawner_get_PropsSpawner = AccessTools.PropertyGetter(typeof(CompSpawner), nameof(CompSpawner.PropsSpawner));

    private static readonly MethodInfo CompImprovedSpawner_IsActive = AccessTools.Method(typeof(CompImprovedSpawner), nameof(IsActive));

    [HarmonyReversePatch]
    [HarmonyPatch("TickInterval")]
    private static void ReverseTickInterval(CompImprovedSpawner @this, int interval)
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.SearchForward(i => i.opcode == OpCodes.Call && i.operand is MethodInfo m && m == CompSpawner_get_PropsSpawner);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch TickInterval, IL does not match expectations: call to get property PropsSpawner not found.");
                return codeMatcher.Instructions();
            }

            // Fetch the label we need to jump to
            codeMatcher.Advance(2);
            var label = codeMatcher.Operand;

            // Go back
            codeMatcher.Advance(-2);

            // Inject ourselves
            codeMatcher.Insert([
                new(OpCodes.Callvirt, CompImprovedSpawner_IsActive),
                new(OpCodes.Brtrue_S, label),
                new(OpCodes.Ret),
                new(OpCodes.Ldarg_0),
            ]);

            return codeMatcher.Instructions();
        }

        // Make compiler happy. This gets patched out anyway.
        _ = @this;
        _ = interval;
        Transpiler(null!, null!);
    }

    private static readonly FieldInfo CompProperties_Spawner_requiresPower = AccessTools.Field(typeof(CompProperties_Spawner), nameof(CompProperties_Spawner.requiresPower));

    [HarmonyReversePatch]
    [HarmonyPatch("CompInspectStringExtra")]
    private static string ReverseCompInspectStringExtra(CompImprovedSpawner @this)
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.SearchForward(i => i.opcode == OpCodes.Ldfld && i.operand is FieldInfo f && f == CompProperties_Spawner_requiresPower);
            if (!codeMatcher.IsValid)
            {
                Log.Error("Could not reverse patch CompInspectStringExtra, IL does not match expectations: access to field CompProperties_Spawner.requiresPower not found.");
                return codeMatcher.Instructions();
            }

            // Fetch the label we need to jump to if false
            codeMatcher.Advance(-3);
            var falseLabel = codeMatcher.Operand;

            // Fetch the label we need to jump to if true
            codeMatcher.Advance(4);
            var trueLabel = codeMatcher.Operand;

            // Go back
            codeMatcher.Advance(-2);

            // Inject ourselves
            codeMatcher.Insert([
                new(OpCodes.Callvirt, CompImprovedSpawner_IsActive),
                new(OpCodes.Brfalse_S, falseLabel),
                new(OpCodes.Br_S, trueLabel),
                new(OpCodes.Ldarg_0),
            ]);

            return codeMatcher.Instructions();
        }

        // Make compiler happy. This gets patched out anyway.
        _ = @this;
        Transpiler(null!, null!);
        return null!;
    }
}

#if !v1_5
using System.Reflection;
using System.Reflection.Emit;

namespace NewAndImprovedXmlComps;

internal class GravshipOrbExtension : DefModExtension
{
#pragma warning disable CS0649 // Set by RimWorld, ensured by ConfigErrors
    public GraphicData? orbGraphic;

#pragma warning restore CS0649

    public GraphicData? cooldownOrbGraphic;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
        {
            yield return error;
        }

        if (orbGraphic == null)
        {
            yield return $"Missing {nameof(orbGraphic)} for {nameof(GravshipOrbExtension)}.";
        }
    }
}

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.DrawAt))]
internal class Building_GravEngine_DrawAt_Patch
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var codeMatcher = new CodeMatcher(instructions, generator);

        _ = codeMatcher.SearchForward(i => i.opcode == OpCodes.Call && i.operand is ConstructorInfo ctor && ctor.DeclaringType == typeof(Vector3));
        if (!codeMatcher.IsValid)
        {
            Log.Error("Could not patch DrawAt, IL does not match expectations: instantiation of Vector3.");
            return codeMatcher.Instructions();
        }

        _ = codeMatcher.Advance(1).Insert([
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldloca_S,0),
            new(OpCodes.Call, AccessTools.Method(typeof(Building_GravEngine_DrawAt_Patch), nameof(UpdateVector3))),
        ]);

        _ = codeMatcher.SearchForward(i => i.Calls(AccessTools.PropertyGetter(typeof(CachedMaterial), nameof(CachedMaterial.Material))));
        if (!codeMatcher.IsValid)
        {
            Log.Error("Could not patch DrawAt, IL does not match expectations: call to CachedMaterial.Material not found.");
            return codeMatcher.Instructions();
        }

        _ = codeMatcher.Advance(1).Insert([
            new(OpCodes.Ldarg_0),
            new(OpCodes.Call, AccessTools.Method(typeof(Building_GravEngine_DrawAt_Patch), nameof(GetOrbGraphic))),
        ]);

        return codeMatcher.Instructions();
    }

    private static void UpdateVector3(Building_GravEngine gravEngine, ref Vector3 vector3)
    {
        var extension = gravEngine.def.GetModExtension<GravshipOrbExtension>();
        if (extension == null || extension.orbGraphic == null)
        {
            return;
        }

        GraphicData orbGraphic = extension.orbGraphic;
        vector3 = new Vector3(orbGraphic.drawSize.x, 1f, orbGraphic.drawSize.y);
    }

    private static Material GetOrbGraphic(Material originalMaterial, Building_GravEngine gravEngine)
    {
        var extension = gravEngine.def.GetModExtension<GravshipOrbExtension>();
        if (extension == null || extension.orbGraphic == null)
        {
            return originalMaterial;
        }

        Graphic graphic =
            extension.cooldownOrbGraphic != null
            && Find.TickManager.TicksGame < gravEngine.cooldownCompleteTick
                ? extension.cooldownOrbGraphic.GraphicColoredFor(gravEngine)
                : extension.orbGraphic.GraphicColoredFor(gravEngine);
        return graphic.MatSingle;
    }
}
#endif

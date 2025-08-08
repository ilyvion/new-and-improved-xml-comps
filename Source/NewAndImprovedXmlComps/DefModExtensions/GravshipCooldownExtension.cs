#if !v1_5
namespace NewAndImprovedXmlComps;

internal class GravshipCooldownExtension : DefModExtension
{
#pragma warning disable CS0649 // Set by RimWorld, ensured by ConfigErrors
    public GraphicData? cooldownGraphic;

#pragma warning restore CS0649

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var error in base.ConfigErrors())
        {
            yield return error;
        }

        if (cooldownGraphic == null)
        {
            yield return $"Missing {nameof(cooldownGraphic)} for {nameof(GravshipCooldownExtension)}.";
        }
    }
}

[HarmonyPatch(typeof(Building_GravEngine))]
[HarmonyPatch(nameof(Building_GravEngine.Graphic), MethodType.Getter)]
internal class Building_GravEngine_Graphic_Patch
{
    private static void Postfix(Building_GravEngine __instance, ref Graphic __result)
    {
        var extension = __instance.def.GetModExtension<GravshipCooldownExtension>();
        if (extension == null || extension.cooldownGraphic == null)
        {
            return;
        }
        if (Find.TickManager.TicksGame < __instance.cooldownCompleteTick)
        {
            __result = extension.cooldownGraphic.GraphicColoredFor(__instance);
        }
    }
}
#endif

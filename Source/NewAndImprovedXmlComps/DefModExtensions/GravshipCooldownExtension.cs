#if !v1_5
namespace NewAndImprovedXmlComps;

internal sealed class GravshipCooldownExtension : DefModExtension
{
#pragma warning disable CS0649 // Set by RimWorld, ensured by ConfigErrors
    public GraphicData? cooldownGraphic;

    public ColorInt? cooldownGlowColor;
    public float? cooldownGlowRadius;
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
internal sealed class Building_GravEngine_Graphic_Patch
{
    internal static void Postfix(Building_GravEngine __instance, ref Graphic __result)
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

[HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.Tick))]
internal sealed class Building_GravEngine_Tick_Patch
{
    internal static void Postfix(Building_GravEngine __instance)
    {
        var extension = __instance.def.GetModExtension<GravshipCooldownExtension>();
        if (
            extension == null
            || !__instance.TryGetComp<CompGlower>(out var compGlower)
            || (extension.cooldownGlowColor == null && extension.cooldownGlowRadius == null)
        )
        {
            return;
        }

        if (Find.TickManager.TicksGame < __instance.cooldownCompleteTick)
        {
            if (extension.cooldownGlowRadius != null && !compGlower.glowRadiusOverride.HasValue)
            {
                compGlower.glowRadiusOverride = extension.cooldownGlowRadius;
            }

            if (extension.cooldownGlowColor.HasValue && !compGlower.HasGlowColorOverride)
            {
                compGlower.SetGlowColorInternal(extension.cooldownGlowColor.Value);
            }
        }
        else
        {
            if (extension.cooldownGlowRadius != null && compGlower.glowRadiusOverride.HasValue)
            {
                compGlower.glowRadiusOverride = null;
            }

            if (extension.cooldownGlowColor.HasValue && compGlower.HasGlowColorOverride)
            {
                compGlower.SetGlowColorInternal(null);
            }
        }
    }
}
#endif

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
        if (extension == null || !__instance.TryGetComp<CompGlower>(out var compGlower))
        {
            return;
        }

        var onCooldown = Find.TickManager.TicksGame < __instance.cooldownCompleteTick;
        var radiusChanged = TryGetOverrideChange(
            onCooldown,
            extension.cooldownGlowRadius,
            compGlower.glowRadiusOverride,
            out var radiusOverride
        );
        var colorChanged = TryGetOverrideChange(
            onCooldown,
            extension.cooldownGlowColor,
            compGlower.glowColorOverride,
            out var colorOverride
        );
        if (!radiusChanged && !colorChanged)
        {
            return;
        }

        // The glow grid copies a glower's radius and color when the glower is registered.
        var glowGrid = __instance.Map.glowGrid;
        glowGrid.DeRegisterGlower(compGlower);
        compGlower.glowRadiusOverride = radiusOverride;
        compGlower.glowColorOverride = colorOverride;
        if (compGlower.Glows)
        {
            glowGrid.RegisterGlower(compGlower);
        }
    }

    /// <summary>
    /// While on cooldown, applies the configured override unless another one is already set;
    /// off cooldown, removes the override only if it is still the configured one.
    /// </summary>
    internal static bool TryGetOverrideChange<T>(
        bool onCooldown,
        T? configured,
        T? current,
        out T? changed
    )
        where T : struct
    {
        changed = current;
        if (configured is not T configuredValue)
        {
            return false;
        }

        if (onCooldown)
        {
            if (current.HasValue)
            {
                return false;
            }
            changed = configuredValue;
            return true;
        }

        if (
            current is not T currentValue
            || !EqualityComparer<T>.Default.Equals(currentValue, configuredValue)
        )
        {
            return false;
        }
        changed = null;
        return true;
    }
}
#endif

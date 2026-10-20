namespace NewAndImprovedXmlComps.Core;

[HarmonyPatch(typeof(CompPowerTrader))]
[HarmonyPatch(nameof(CompPowerTrader.PowerOutput), MethodType.Getter)]
internal static class PowerTraderPatch
{
    internal const int FactorCacheTicks = 250;

    // PowerNet reads PowerOutput every tick for every power trader, so the stat is cached per comp.
    private static readonly ConditionalWeakTable<CompPowerTrader, CachedValue<float>> FactorCache =
#if v1_5
        new();
#else
    [];
#endif

    internal static void Postfix(CompPowerTrader __instance, ref float __result)
    {
        if (__result == 0f || __instance?.parent == null)
        {
            return;
        }

        __result *= FactorCache
            .GetValue(
                __instance,
                static comp => new CachedValue<float>(
                    () =>
                        comp.parent.GetStatValue(
                            PowerOffsetDefOf.NewAndImprovedXmlComps_PowerGenFactor
                        ),
                    FactorCacheTicks
                )
            )
            .Value;
    }
}

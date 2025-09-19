namespace NewAndImprovedXmlComps.Core;

[HarmonyPatch(typeof(CompPowerTrader))]
[HarmonyPatch(nameof(CompPowerTrader.PowerOutput), MethodType.Getter)]
internal static class PowerTraderPatch
{
    internal static void Postfix(CompPowerTrader __instance, ref float __result)
    {
        var multi =
            __instance?.parent?.GetStatValue(PowerOffsetDefOf.NewAndImprovedXmlComps_PowerGenFactor)
            ?? 1f;
        __result *= multi;
    }
}

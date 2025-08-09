namespace NewAndImprovedXmlComps.Core;

[DefOf]
public static class PowerOffsetDefOf
{
    public static StatDef NewAndImprovedXmlComps_PowerGenFactor;

#pragma warning disable CS8618 // Set by RimWorld
    static PowerOffsetDefOf()
#pragma warning restore CS8618
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(PowerOffsetDefOf));
    }
}


[HarmonyPatch(typeof(CompPowerTrader))]
[HarmonyPatch(nameof(CompPowerTrader.PowerOutput), MethodType.Getter)]
public static class PowerTraderPatch
{
    internal static void Postfix(CompPowerTrader __instance, ref float __result)
    {
        float multi = __instance?.parent?.GetStatValue(PowerOffsetDefOf.NewAndImprovedXmlComps_PowerGenFactor) ?? 1f;
        __result *= multi;
    }
}

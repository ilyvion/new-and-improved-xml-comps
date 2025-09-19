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

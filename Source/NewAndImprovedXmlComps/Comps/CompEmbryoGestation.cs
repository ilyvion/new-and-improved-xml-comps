namespace NewAndImprovedXmlComps;

public partial class CompEmbryoGestation : ThingComp
{
    public CompProperties_EmbryoGestation PropsEmbryoGestation => (CompProperties_EmbryoGestation)props;
}

public class CompProperties_EmbryoGestation : CompProperties
{
    public int embryoGestationTicks = Building_GrowthVat.EmbryoGestationTicks;

    public CompProperties_EmbryoGestation()
    {
        compClass = typeof(CompEmbryoGestation);
    }
}

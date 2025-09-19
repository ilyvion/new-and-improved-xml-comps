namespace NewAndImprovedXmlComps;

/// <summary>
/// Properties for embryo gestation component.
/// </summary>
public class CompProperties_EmbryoGestation : CompProperties
{
    /// <summary>
    /// Number of ticks required for embryo gestation.
    /// </summary>
    public int embryoGestationTicks = Building_GrowthVat.EmbryoGestationTicks;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompProperties_EmbryoGestation"/> class.
    /// </summary>
    public CompProperties_EmbryoGestation()
    {
        compClass = typeof(CompEmbryoGestation);
    }
}

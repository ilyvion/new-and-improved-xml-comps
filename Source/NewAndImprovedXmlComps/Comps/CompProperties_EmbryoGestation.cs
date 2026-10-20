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

    /// <inheritdoc/>
    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
        {
            yield return error;
        }

        if (embryoGestationTicks <= 0)
        {
            yield return $"{nameof(embryoGestationTicks)} must be greater than 0, but is {embryoGestationTicks}.";
        }
    }
}

namespace NewAndImprovedXmlComps;

/// <summary>
/// Component for handling embryo gestation logic.
/// </summary>
public partial class CompEmbryoGestation : ThingComp
{
    /// <summary>
    /// Gets the embryo gestation properties for this component.
    /// </summary>
    public CompProperties_EmbryoGestation PropsEmbryoGestation =>
        (CompProperties_EmbryoGestation)props;
}

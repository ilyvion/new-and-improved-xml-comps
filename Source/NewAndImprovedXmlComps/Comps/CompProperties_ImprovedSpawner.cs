namespace NewAndImprovedXmlComps;

/// <summary>
/// Defines properties for the improved spawner component.
/// </summary>
public class CompProperties_ImprovedSpawner : CompProperties_Spawner
{
    /// <summary>
    /// Indicates whether the spawner requires fuel to operate.
    /// </summary>
    public bool requiresFuel;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompProperties_ImprovedSpawner"/> class.
    /// </summary>
    public CompProperties_ImprovedSpawner()
    {
        compClass = typeof(CompImprovedSpawner);
    }
}

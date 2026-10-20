namespace NewAndImprovedXmlComps;

/// <summary>
/// Provides improved spawner functionality
/// </summary>
public partial class CompImprovedSpawner : CompSpawner
{
    /// <summary>
    /// Gets the improved spawner properties for this component.
    /// </summary>
    public new CompProperties_ImprovedSpawner PropsSpawner => (CompProperties_ImprovedSpawner)props;
    private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

    private bool HasFuel => parent.GetComp<CompRefuelable>()?.HasFuel ?? false;

    /// <inheritdoc/>
    public override void CompTick() => TickIntervalDelta(1);

    /// <inheritdoc/>
    public override void CompTickRare() => TickIntervalDelta(250);

#if v1_5
    private void TickIntervalDelta(int interval)
#else
    private new void TickIntervalDelta(int interval)
#endif
        => ReverseTickIntervalDelta(this, interval);

    /// <summary>
    /// Determines whether the spawner is currently active based on power and fuel requirements.
    /// </summary>
    /// <returns>True if the spawner is active; otherwise, false.</returns>
    protected virtual bool IsActive() =>
        IsActive(
            PropsSpawner.requiresPower,
            PropsSpawner.requiresFuel,
            () => PowerOn,
            () => HasFuel
        );

    internal static bool IsActive(
        bool requiresPower,
        bool requiresFuel,
        Func<bool> powerOn,
        Func<bool> hasFuel
    ) =>
        (!requiresPower && !requiresFuel)
        || (requiresPower && requiresFuel && powerOn() && hasFuel())
        || (requiresPower && !requiresFuel && powerOn())
        || (!requiresPower && requiresFuel && hasFuel());

    /// <inheritdoc/>
    public override string CompInspectStringExtra() => ReverseCompInspectStringExtra(this);
}

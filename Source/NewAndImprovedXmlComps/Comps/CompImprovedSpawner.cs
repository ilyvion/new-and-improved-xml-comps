namespace NewAndImprovedXmlComps;

public partial class CompImprovedSpawner : CompSpawner
{
    public new CompProperties_ImprovedSpawner PropsSpawner => (CompProperties_ImprovedSpawner)props;
    private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

    private bool HasFuel => parent.GetComp<CompRefuelable>()?.HasFuel ?? false;

    public override void CompTick()
    {
        TickInterval(1);
    }

    public override void CompTickRare()
    {
        TickInterval(250);
    }

    private void TickInterval(int interval)
    {
        ReverseTickInterval(this, interval);
    }

    protected virtual bool IsActive()
    {
        bool requiresPower = PropsSpawner.requiresPower;
        bool requiresFuel = PropsSpawner.requiresFuel;

        return (!requiresPower && !requiresFuel)
            || (requiresPower && requiresFuel && PowerOn && HasFuel)
            || (requiresPower && !requiresFuel && PowerOn)
            || (!requiresPower && requiresFuel && HasFuel);
    }

    public override string CompInspectStringExtra()
    {
        return ReverseCompInspectStringExtra(this);
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Performance",
    "CA1051",
    Justification = "CompProperties require visible instance fields")]
public class CompProperties_ImprovedSpawner : CompProperties_Spawner
{
    public bool requiresFuel;

    public CompProperties_ImprovedSpawner()
    {
        compClass = typeof(CompImprovedSpawner);
    }
}

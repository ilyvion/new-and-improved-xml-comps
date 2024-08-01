namespace NewAndImprovedXmlComps;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Performance",
    "CA1051",
    Justification = "CompProperties require visible instance fields")]
public class CompProperties_ImprovedPower : CompProperties_Power
{
    public HideWireMode hideWireMode;

    internal static bool ShouldPrintWirePieceConnecting(CompPower @this)
    {
        if (@this.props is not CompProperties_ImprovedPower compPropertiesImprovedPower)
        {
            return true;
        }

        return compPropertiesImprovedPower.hideWireMode != HideWireMode.Always;
    }

    internal static bool ShouldPostPrintOnto(CompPower @this)
    {
        if (@this.props is not CompProperties_ImprovedPower compPropertiesImprovedPower)
        {
            return true;
        }

        return compPropertiesImprovedPower.hideWireMode == HideWireMode.Never;
    }
}

public enum HideWireMode
{
    Never = 0,
    Normally = 1,
    Always = 2
}

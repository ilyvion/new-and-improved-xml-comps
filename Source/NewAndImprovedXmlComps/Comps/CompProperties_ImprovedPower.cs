namespace NewAndImprovedXmlComps;

/// <summary>
/// Defines improved power properties for a component, including wire hiding behavior.
/// </summary>
public class CompProperties_ImprovedPower : CompProperties_Power
{
    /// <summary>
    /// Specifies how power wires should be hidden for this component.
    /// </summary>
    public HideWireMode hideWireMode;

    internal static bool ShouldPrintWirePieceConnecting(CompPower @this) =>
        @this.props is not CompProperties_ImprovedPower compPropertiesImprovedPower
        || compPropertiesImprovedPower.hideWireMode != HideWireMode.Always;

    internal static bool ShouldPostPrintOnto(CompPower @this) =>
        @this.props is not CompProperties_ImprovedPower compPropertiesImprovedPower
        || compPropertiesImprovedPower.hideWireMode == HideWireMode.Never;
}

/// <summary>
/// Specifies the mode for hiding power wires.
/// </summary>
public enum HideWireMode
{
    /// <summary>
    /// Never hide power wires; wires are always visible.
    /// </summary>
    Never = 0,

    /// <summary>
    /// Hide power wires when power grid is not visible.
    /// </summary>
    Normally = 1,

    /// <summary>
    /// Always hide power wires; wires are never visible.
    /// </summary>
    Always = 2,
}

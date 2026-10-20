using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class HideWireModeTests
{
    private static CompPowerTrader MakeImprovedPower(HideWireMode mode) =>
        new() { props = new CompProperties_ImprovedPower { hideWireMode = mode } };

    [TestCase(HideWireMode.Never, ExpectedResult = true)]
    [TestCase(HideWireMode.Normally, ExpectedResult = true)]
    [TestCase(HideWireMode.Always, ExpectedResult = false)]
    public static bool PowerOverlayWireShownUnlessAlwaysHidden(HideWireMode mode) =>
        CompProperties_ImprovedPower.ShouldPrintWirePieceConnecting(MakeImprovedPower(mode));

    [TestCase(HideWireMode.Never, ExpectedResult = true)]
    [TestCase(HideWireMode.Normally, ExpectedResult = false)]
    [TestCase(HideWireMode.Always, ExpectedResult = false)]
    public static bool MapWireShownOnlyWhenNeverHidden(HideWireMode mode) =>
        CompProperties_ImprovedPower.ShouldPostPrintOnto(MakeImprovedPower(mode));

    [Test]
    public static void VanillaPowerPropsShowWiresEverywhere()
    {
        var power = new CompPowerTrader { props = new CompProperties_Power() };
        Expect.IsTrue(CompProperties_ImprovedPower.ShouldPrintWirePieceConnecting(power));
        Expect.IsTrue(CompProperties_ImprovedPower.ShouldPostPrintOnto(power));
    }

    // A def that omits <hideWireMode> must behave like vanilla.
    [Test]
    public static void DefaultModeIsNever() =>
        Expect.AreEqual(HideWireMode.Never, new CompProperties_ImprovedPower().hideWireMode);
}

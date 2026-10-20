using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class EmbryoGestationConfigErrorsTests
{
    private static List<string> ConfigErrors(int embryoGestationTicks)
    {
        var props = new CompProperties_EmbryoGestation
        {
            embryoGestationTicks = embryoGestationTicks,
        };
        var parentDef = new ThingDef { defName = "NIXC_TestGrowthVat" };
        parentDef.comps.Add(props);
        return [.. props.ConfigErrors(parentDef)];
    }

    [Test]
    public static void PositiveGestationTicksHaveNoErrors() => Expect.IsEmpty(ConfigErrors(1));

    [Test]
    public static void DefaultGestationTicksHaveNoErrors() =>
        Expect.IsEmpty(ConfigErrors(Building_GrowthVat.EmbryoGestationTicks));

    [Test]
    public static void ZeroGestationTicksReportIt()
    {
        var errors = ConfigErrors(0);

        Expect.AreEqual(1, errors.Count);
        Expect.AreEqual("embryoGestationTicks must be greater than 0, but is 0.", errors[0]);
    }

    [Test]
    public static void NegativeGestationTicksReportIt()
    {
        var errors = ConfigErrors(-100);

        Expect.AreEqual(1, errors.Count);
        Expect.AreEqual("embryoGestationTicks must be greater than 0, but is -100.", errors[0]);
    }
}

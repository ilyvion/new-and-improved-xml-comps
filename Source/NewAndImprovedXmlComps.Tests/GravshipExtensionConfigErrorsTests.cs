using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class GravshipExtensionConfigErrorsTests
{
    [Test]
    public static void OrbExtensionWithoutAnOrbGraphicReportsIt()
    {
        var errors = new GravshipOrbExtension { cooldownOrbGraphic = new GraphicData() }
            .ConfigErrors()
            .ToList();

        Expect.AreEqual(1, errors.Count);
        Expect.AreEqual("Missing orbGraphic for GravshipOrbExtension.", errors[0]);
    }

    [Test]
    public static void OrbExtensionWithAnOrbGraphicHasNoErrors() =>
        Expect.IsEmpty(new GravshipOrbExtension { orbGraphic = new GraphicData() }.ConfigErrors());

    [Test]
    public static void CooldownExtensionWithoutACooldownGraphicReportsIt()
    {
        var errors = new GravshipCooldownExtension().ConfigErrors().ToList();

        Expect.AreEqual(1, errors.Count);
        Expect.AreEqual("Missing cooldownGraphic for GravshipCooldownExtension.", errors[0]);
    }

    // The glow fields don't stand in for the required cooldownGraphic.
    [Test]
    public static void CooldownExtensionWithOnlyGlowFieldsStillReportsTheMissingGraphic()
    {
        var errors = new GravshipCooldownExtension
        {
            cooldownGlowColor = new ColorInt(255, 218, 0, 0),
            cooldownGlowRadius = 40f,
        }
            .ConfigErrors()
            .ToList();

        Expect.AreEqual(1, errors.Count);
        Expect.AreEqual("Missing cooldownGraphic for GravshipCooldownExtension.", errors[0]);
    }

    [Test]
    public static void CooldownExtensionWithACooldownGraphicHasNoErrors() =>
        Expect.IsEmpty(
            new GravshipCooldownExtension { cooldownGraphic = new GraphicData() }.ConfigErrors()
        );
}

using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class CompPropertiesDefaultsTests
{
    [Test]
    public static void ImprovedSpawnerPropsDefaultToTheImprovedSpawnerWithoutFuel()
    {
        var props = new CompProperties_ImprovedSpawner();
        Expect.AreEqual(typeof(CompImprovedSpawner), props.compClass);
        Expect.IsFalse(props.requiresFuel);
    }

    [Test]
    public static void EmbryoGestationPropsDefaultToTheVanillaDuration()
    {
        var props = new CompProperties_EmbryoGestation();
        Expect.AreEqual(typeof(CompEmbryoGestation), props.compClass);
        Expect.AreEqual(540000, props.embryoGestationTicks);
    }

    [Test]
    public static void PropsSpawnerIsTheCompsProps()
    {
        var props = new CompProperties_ImprovedSpawner();
        var comp = new CompImprovedSpawner { props = props };
        Expect.ReferencesAreEqual(props, comp.PropsSpawner);
    }

    [Test]
    public static void PropsEmbryoGestationIsTheCompsProps()
    {
        var props = new CompProperties_EmbryoGestation();
        var comp = new CompEmbryoGestation { props = props };
        Expect.ReferencesAreEqual(props, comp.PropsEmbryoGestation);
    }

    // Guards the XML-to-field mapping of requiresFuel.
    [Test]
    public static void ShippedSpawnerTestDefRequiresFuel() =>
        Expect.IsTrue(
            DefDatabase<ThingDef>
                .GetNamed("NIXC_ImprovedSpawnerTest")
                .GetCompProperties<CompProperties_ImprovedSpawner>()
                .requiresFuel
        );
}

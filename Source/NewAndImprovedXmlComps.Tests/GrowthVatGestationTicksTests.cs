using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

// GetEmbryoGestationTicks and SortByEmbryoGestationTicks each resolve a vat's gestation duration;
// both must agree on the override and on the vanilla fallback.
[TestFixture(TestType.MainMenu)]
internal sealed class GrowthVatGestationTicksTests
{
    private static Building_GrowthVat MakeVat(params CompProperties[] comps)
    {
        var def = new ThingDef
        {
            defName = "NIXC_GestationTicksTestVat",
            thingClass = typeof(Building_GrowthVat),
        };
        def.comps.AddRange(comps);
        var vat = new Building_GrowthVat { def = def };
        vat.InitializeComps();
        return vat;
    }

    private static void ExpectGestationTicks(int expected, Building_GrowthVat vat)
    {
        Expect.AreEqual(
            expected,
            CompEmbryoGestation.GetEmbryoGestationTicks(vat),
            nameof(CompEmbryoGestation.GetEmbryoGestationTicks)
        );
        Expect.AreEqual(
            expected,
            HumanEmbryo_BestAvailableGrowthVat_Patch.SortByEmbryoGestationTicks()(vat),
            nameof(HumanEmbryo_BestAvailableGrowthVat_Patch.SortByEmbryoGestationTicks)
        );
    }

    [Test]
    public static void AVatWithTheCompUsesItsGestationTicks() =>
        ExpectGestationTicks(
            1234,
            MakeVat(new CompProperties_EmbryoGestation { embryoGestationTicks = 1234 })
        );

    [Test]
    public static void AVatWithoutTheCompUsesTheVanillaDuration() =>
        ExpectGestationTicks(Building_GrowthVat.EmbryoGestationTicks, MakeVat());

    [Test]
    public static void TheCompDefaultsToTheVanillaDuration() =>
        ExpectGestationTicks(
            Building_GrowthVat.EmbryoGestationTicks,
            MakeVat(new CompProperties_EmbryoGestation())
        );

    [Test]
    public static void NoVatUsesTheVanillaDuration() =>
        Expect.AreEqual(
            Building_GrowthVat.EmbryoGestationTicks,
            CompEmbryoGestation.GetEmbryoGestationTicks(null)
        );
}

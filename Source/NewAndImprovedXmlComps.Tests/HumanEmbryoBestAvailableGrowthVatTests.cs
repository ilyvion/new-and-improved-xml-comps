using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

// An embryo picks a powered vat first, then the one with the shortest gestation, then the nearest.
[TestFixture(TestType.Playing)]
[LoadIfBiotechActive]
internal sealed class HumanEmbryoBestAvailableGrowthVatTests
{
    private static readonly IntVec3 Near = new(3, 0, 0);
    private static readonly IntVec3 Far = new(10, 0, 0);

    [Test]
    public static void WithNoVatThereIsNoBestVat()
    {
        using var scene = new GrowthVatScene(
            Find.CurrentMap,
            Building_GrowthVat.EmbryoGestationTicks
        );
        var embryo = scene.SpawnEmbryo();

        Expect.IsNull(embryo.BestAvailableGrowthVat());
    }

    [Test]
    public static void AShorterGestationWinsOverDistance()
    {
        using var scene = new GrowthVatScene(
            Find.CurrentMap,
            Building_GrowthVat.EmbryoGestationTicks
        );
        var embryo = scene.SpawnEmbryo();
        _ = scene.SpawnVat(Near, embryoGestationTicks: 2 * GenDate.TicksPerDay);
        var far = scene.SpawnVat(Far, embryoGestationTicks: GenDate.TicksPerDay);

        Expect.ReferencesAreEqual(far, embryo.BestAvailableGrowthVat());
    }

    [Test]
    public static void WithEqualGestationTheNearestWins()
    {
        using var scene = new GrowthVatScene(Find.CurrentMap, GenDate.TicksPerDay);
        var embryo = scene.SpawnEmbryo();
        var near = scene.SpawnVat(Near);
        _ = scene.SpawnVat(Far);

        Expect.ReferencesAreEqual(near, embryo.BestAvailableGrowthVat());
    }

    [Test]
    public static void PowerWinsOverAShorterGestation()
    {
        using var scene = new GrowthVatScene(
            Find.CurrentMap,
            Building_GrowthVat.EmbryoGestationTicks
        );
        var embryo = scene.SpawnEmbryo();
        _ = scene.SpawnVat(Near, powered: false, embryoGestationTicks: GenDate.TicksPerDay);
        var powered = scene.SpawnVat(Far, embryoGestationTicks: 2 * GenDate.TicksPerDay);

        Expect.ReferencesAreEqual(powered, embryo.BestAvailableGrowthVat());
    }
}

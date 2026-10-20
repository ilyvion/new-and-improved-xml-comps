using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.Playing)]
[LoadIfBiotechActive]
internal sealed class GrowthVatTryGrowEmbryoTests
{
    private static void ExpectGestationEndsAfter(int expectedTicks, int? embryoGestationTicks)
    {
        using var scene = new GrowthVatScene(Find.CurrentMap, embryoGestationTicks);
        var vat = scene.SpawnVat(IntVec3.Zero);
        _ = scene.LoadEmbryo(vat);
        var now = Find.TickManager.TicksGame;

        vat.TryGrowEmbryo();

        Expect.AreEqual(now + expectedTicks, vat.startTick);
    }

    [Test]
    public static void WithoutTheCompGestationTakesTheVanillaDuration() =>
        ExpectGestationEndsAfter(Building_GrowthVat.EmbryoGestationTicks, null);

    [Test]
    public static void WithTheCompGestationTakesTheCustomDuration() =>
        ExpectGestationEndsAfter(GenDate.TicksPerDay, GenDate.TicksPerDay);
}

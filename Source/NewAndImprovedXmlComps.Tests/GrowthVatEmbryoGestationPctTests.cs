using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

// EmbryoGestationPct sizes the fetus drawn in the vat, so it must measure progress against the
// vat's own gestation duration.
[TestFixture(TestType.Playing)]
[LoadIfBiotechActive]
internal sealed class GrowthVatEmbryoGestationPctTests
{
    private sealed class VatScene : IDisposable
    {
        private readonly CompProperties_EmbryoGestation? _addedCompProperties;

        public Building_GrowthVat Vat { get; }

        public VatScene(Map map, int? embryoGestationTicks = null)
        {
            Expect.IsEmpty(
                map.listerThings.ThingsOfDef(ThingDefOf.GrowthVat),
                "growth vats left behind by an earlier test"
            );

            if (embryoGestationTicks is int ticks)
            {
                _addedCompProperties = new CompProperties_EmbryoGestation
                {
                    embryoGestationTicks = ticks,
                };
                ThingDefOf.GrowthVat.comps.Add(_addedCompProperties);
            }

            var vat = ThingMaker.MakeThing(ThingDefOf.GrowthVat);
            vat.SetFaction(Faction.OfPlayer);
            Vat = (Building_GrowthVat)GenSpawn.Spawn(vat, map.Center, map);
        }

        public float PctWithTicksRemaining(int ticksRemaining)
        {
            Vat.startTick = Find.TickManager.TicksGame + ticksRemaining;
            return Vat.EmbryoGestationPct;
        }

        public void Dispose()
        {
            if (!Vat.Destroyed)
            {
                Vat.Destroy();
            }
            if (_addedCompProperties != null)
            {
                _ = ThingDefOf.GrowthVat.comps.Remove(_addedCompProperties);
            }
        }
    }

    [Test]
    public static void WithoutTheCompProgressIsMeasuredAgainstTheVanillaDuration()
    {
        using var scene = new VatScene(Find.CurrentMap);
        const int Ticks = Building_GrowthVat.EmbryoGestationTicks;

        Expect.AreApproximatelyEqual(0f, scene.PctWithTicksRemaining(Ticks));
        Expect.AreApproximatelyEqual(0.5f, scene.PctWithTicksRemaining(Ticks / 2));
        Expect.AreApproximatelyEqual(1f, scene.PctWithTicksRemaining(0));
    }

    [Test]
    public static void WithTheCompProgressIsMeasuredAgainstTheCustomDuration()
    {
        const int Ticks = GenDate.TicksPerDay;
        using var scene = new VatScene(Find.CurrentMap, Ticks);

        Expect.AreApproximatelyEqual(0f, scene.PctWithTicksRemaining(Ticks));
        Expect.AreApproximatelyEqual(0.5f, scene.PctWithTicksRemaining(Ticks / 2));
        Expect.AreApproximatelyEqual(1f, scene.PctWithTicksRemaining(0));
    }

    [Test]
    public static void WithACustomDurationLongerThanVanillaProgressStartsAtZero()
    {
        const int Ticks = Building_GrowthVat.EmbryoGestationTicks * 2;
        using var scene = new VatScene(Find.CurrentMap, Ticks);

        Expect.AreApproximatelyEqual(0f, scene.PctWithTicksRemaining(Ticks));
        Expect.AreApproximatelyEqual(0.5f, scene.PctWithTicksRemaining(Ticks / 2));
    }
}

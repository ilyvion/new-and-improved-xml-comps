using DevTools.Testing;
using NewAndImprovedXmlComps.Core;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class PowerGenFactorStatTests
{
    [Test]
    public static void TheStatDefIsLoaded() =>
        Expect.IsNotNull(PowerOffsetDefOf.NewAndImprovedXmlComps_PowerGenFactor);

    [Test]
    public static void APowerCompWithoutAParentKeepsItsOutput()
    {
        var output = 1000f;

        PowerTraderPatch.Postfix(new CompPowerTrader(), ref output);

        Expect.AreApproximatelyEqual(1000f, output);
    }
}

[TestFixture(TestType.Playing)]
internal sealed class PowerGenFactorTests
{
    // NIXC_CustomLinkable's NewAndImprovedXmlComps_PowerGenFactor offset is +0.1.
    private const float BaseOutput = 1000f;

    private static readonly IntVec3[] FacilityOffsets = [new(0, 0, 4), new(0, 0, -4)];

    private sealed class GeneratorScene : IDisposable
    {
        private readonly Map _map;
        private readonly List<Thing> _spawned = [];

        public Building Generator { get; }
        public CompPowerTrader PowerTrader { get; }
        public List<Thing> Facilities { get; } = [];

        public GeneratorScene(Map map, int facilityCount)
        {
            _map = map;
            var generatorDef = DefDatabase<ThingDef>.GetNamed("NIXC_WoodFiredGeneratorCustom");
            var facilityDef = DefDatabase<ThingDef>.GetNamed("NIXC_CustomLinkable");
            Expect.IsEmpty(
                map.listerThings.ThingsOfDef(generatorDef),
                "generators left behind by an earlier test"
            );
            Expect.IsEmpty(
                map.listerThings.ThingsOfDef(facilityDef),
                "facilities left behind by an earlier test"
            );

            Generator = (Building)Spawn(generatorDef, map.Center);
            for (var i = 0; i < facilityCount; i++)
            {
                Facilities.Add(Spawn(facilityDef, map.Center + FacilityOffsets[i]));
            }

            PowerTrader = Generator.GetComp<CompPowerTrader>();
            Expect.AreEqual(
                facilityCount,
                Generator.GetComp<CompAffectedByFacilities>().LinkedFacilitiesListForReading.Count,
                "linked facilities"
            );
        }

        private Thing Spawn(ThingDef def, IntVec3 cell)
        {
            var thing = ThingMaker.MakeThing(def);
            thing.SetFactionDirect(Faction.OfPlayer);
            _ = GenSpawn.Spawn(thing, cell, _map);
            _spawned.Add(thing);
            return thing;
        }

        public void Dispose()
        {
            foreach (var thing in _spawned)
            {
                if (!thing.Destroyed)
                {
                    thing.Destroy();
                }
            }
        }
    }

    private static float OutputWithFacilities(int facilityCount, float output)
    {
        using var scene = new GeneratorScene(Find.CurrentMap, facilityCount);
        scene.PowerTrader.PowerOutput = output;
        return scene.PowerTrader.PowerOutput;
    }

    [Test]
    public static void WithoutAFacilityTheOutputIsUnchanged() =>
        Expect.AreApproximatelyEqual(BaseOutput, OutputWithFacilities(0, BaseOutput));

    [Test]
    public static void OneFacilityRaisesTheOutputByItsOffset() =>
        Expect.AreApproximatelyEqual(1100f, OutputWithFacilities(1, BaseOutput));

    [Test]
    public static void TheOffsetsOfSeveralFacilitiesAddUp() =>
        Expect.AreApproximatelyEqual(1200f, OutputWithFacilities(2, BaseOutput));

    [Test]
    public static void PowerConsumptionIsScaledTheSameWay() =>
        Expect.AreApproximatelyEqual(-1100f, OutputWithFacilities(1, -BaseOutput));

#if v1_7_OR_GREATER
    // On 1.7 the JIT can inline the small PowerOutput getter into its callers, bypassing the patch.
    [Test]
    public static void VanillaCallersOfPowerOutputSeeTheScaledOutput()
    {
        using var scene = new GeneratorScene(Find.CurrentMap, 1);
        scene.PowerTrader.PowerOutput = BaseOutput;

        Expect.AreApproximatelyEqual(
            1100f * CompPower.WattsToWattDaysPerTick,
            scene.PowerTrader.EnergyOutputPerTick
        );
    }
#endif

    [Test]
    public static void AFactorChangeTakesEffectOnceTheCachedFactorGoesStale()
    {
        using var scene = new GeneratorScene(Find.CurrentMap, 1);
        scene.PowerTrader.PowerOutput = BaseOutput;
        Expect.AreApproximatelyEqual(1100f, scene.PowerTrader.PowerOutput, "before unlinking");

        scene.Facilities[0].Destroy();
        Expect.AreApproximatelyEqual(1100f, scene.PowerTrader.PowerOutput, "within the cache");

        using var _ = new MockGameTicks(
            Find.TickManager.TicksGame + PowerTraderPatch.FactorCacheTicks + 1
        );
        Expect.AreApproximatelyEqual(BaseOutput, scene.PowerTrader.PowerOutput, "after the cache");
    }

    [Test]
    public static void AnEmpStunnedGeneratorProducesNothing()
    {
        using var scene = new GeneratorScene(Find.CurrentMap, 1);
        scene.PowerTrader.PowerOutput = BaseOutput;
        var stunHandler = scene.Generator.GetComp<CompStunnable>().StunHandler;
        stunHandler.StunFor(1000, null, addBattleLog: false, showMote: false);
        stunHandler.stunFromEMP = true;

        Expect.AreApproximatelyEqual(0f, scene.PowerTrader.PowerOutput);
    }
}

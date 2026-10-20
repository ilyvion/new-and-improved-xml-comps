using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.Playing)]
[LoadIfBiotechActive]
internal sealed class HumanEmbryoGetGizmosTests
{
    private static Command_Action? FindInsertGrowthVatGizmo(HumanEmbryo embryo)
    {
        var label = "InsertGrowthVatLabel".Translate() + "...";
        return embryo
            .GetGizmos()
            .ToList()
            .OfType<Command_Action>()
            .FirstOrDefault(g => g.defaultLabel == label);
    }

    private sealed class EmbryoScene : IDisposable
    {
        private readonly bool _noBabiesOrChildren;
        private readonly CompProperties_EmbryoGestation? _addedCompProperties;
        private readonly List<Thing> _spawned = [];

        public Map Map { get; }
        public HumanEmbryo Embryo { get; }

        public EmbryoScene(Map map, int? embryoGestationTicks = null)
        {
            Map = map;
            Expect.IsEmpty(
                map.listerThings.ThingsOfDef(ThingDefOf.GrowthVat),
                "growth vats left behind by an earlier test"
            );

            var difficulty = Find.Storyteller.difficulty;
            _noBabiesOrChildren = difficulty.noBabiesOrChildren;
            difficulty.noBabiesOrChildren = false;

            if (embryoGestationTicks is int ticks)
            {
                _addedCompProperties = new CompProperties_EmbryoGestation
                {
                    embryoGestationTicks = ticks,
                };
                ThingDefOf.GrowthVat.comps.Add(_addedCompProperties);
            }

            Embryo = (HumanEmbryo)Spawn(ThingMaker.MakeThing(ThingDefOf.HumanEmbryo), map.Center);
        }

        public Building_GrowthVat SpawnGrowthVat()
        {
            var vat = ThingMaker.MakeThing(ThingDefOf.GrowthVat);
            vat.SetFaction(Faction.OfPlayer);
            return (Building_GrowthVat)Spawn(vat, Map.Center + new IntVec3(5, 0, 5));
        }

        private Thing Spawn(Thing thing, IntVec3 cell)
        {
            var spawned = GenSpawn.Spawn(thing, cell, Map);
            _spawned.Add(spawned);
            return spawned;
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
            if (_addedCompProperties != null)
            {
                _ = ThingDefOf.GrowthVat.comps.Remove(_addedCompProperties);
            }
            Find.Storyteller.difficulty.noBabiesOrChildren = _noBabiesOrChildren;
        }
    }

    // Regression guard: the patched gizmo description looked up the gestation ticks on the best
    // available vat without checking for null, so an embryo with no free growth vat threw.
    [Test]
    public static void WithNoAvailableVatTheInsertGizmoIsDisabledInsteadOfThrowing()
    {
        using var scene = new EmbryoScene(Find.CurrentMap);

        var gizmo = FindInsertGrowthVatGizmo(scene.Embryo);

        Expect.IsNotNull(gizmo);
        if (gizmo == null)
        {
            return;
        }
        Expect.IsTrue(gizmo.Disabled);
        Expect.IsTrue(
            gizmo.defaultDesc.Contains(
                Building_GrowthVat.EmbryoGestationTicks.ToStringTicksToPeriod(),
                StringComparison.Ordinal
            )
        );
    }

    [Test]
    public static void WithAVatWithoutTheCompTheDescriptionUsesTheVanillaDuration()
    {
        using var scene = new EmbryoScene(Find.CurrentMap);
        _ = scene.SpawnGrowthVat();

        var gizmo = FindInsertGrowthVatGizmo(scene.Embryo);

        Expect.IsNotNull(gizmo);
        if (gizmo == null)
        {
            return;
        }
        Expect.IsFalse(gizmo.Disabled);
        Expect.IsTrue(
            gizmo.defaultDesc.Contains(
                Building_GrowthVat.EmbryoGestationTicks.ToStringTicksToPeriod(),
                StringComparison.Ordinal
            )
        );
    }

    [Test]
    public static void WithAVatWithTheCompTheDescriptionUsesTheCustomDuration()
    {
        const int EmbryoGestationTicks = GenDate.TicksPerDay;
        using var scene = new EmbryoScene(Find.CurrentMap, EmbryoGestationTicks);
        _ = scene.SpawnGrowthVat();

        var gizmo = FindInsertGrowthVatGizmo(scene.Embryo);

        Expect.IsNotNull(gizmo);
        if (gizmo == null)
        {
            return;
        }
        Expect.IsFalse(gizmo.Disabled);
        Expect.IsTrue(
            gizmo.defaultDesc.Contains(
                EmbryoGestationTicks.ToStringTicksToPeriod(),
                StringComparison.Ordinal
            )
        );
        Expect.IsFalse(
            gizmo.defaultDesc.Contains(
                Building_GrowthVat.EmbryoGestationTicks.ToStringTicksToPeriod(),
                StringComparison.Ordinal
            )
        );
    }
}

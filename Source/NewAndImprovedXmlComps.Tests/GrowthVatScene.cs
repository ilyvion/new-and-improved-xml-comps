using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

// A map with player-owned growth vats and human embryos, optionally with
// CompProperties_EmbryoGestation added to the growth vat def. Requires Biotech.
internal sealed class GrowthVatScene : IDisposable
{
    private readonly bool _noBabiesOrChildren;
    private readonly CompProperties_EmbryoGestation? _addedCompProperties;
    private readonly List<Building_GrowthVat> _vats = [];
    private readonly List<HumanEmbryo> _embryos = [];

    public Map Map { get; }

    public GrowthVatScene(Map map, int? embryoGestationTicks = null)
    {
        Map = map;
        Expect.IsEmpty(
            map.listerThings.ThingsOfDef(ThingDefOf.GrowthVat),
            "growth vats left behind by an earlier test"
        );
        Expect.IsEmpty(
            map.listerThings.ThingsOfDef(ThingDefOf.HumanEmbryo),
            "embryos left behind by an earlier test"
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
    }

    // embryoGestationTicks gives this vat its own duration; it needs the scene to have added the
    // comp to the def.
    public Building_GrowthVat SpawnVat(
        IntVec3 offset,
        bool powered = true,
        int? embryoGestationTicks = null
    )
    {
        var thing = ThingMaker.MakeThing(ThingDefOf.GrowthVat);
        thing.SetFaction(Faction.OfPlayer);
        var vat = (Building_GrowthVat)GenSpawn.Spawn(thing, Map.Center + offset, Map);
        _vats.Add(vat);

        vat.GetComp<CompPowerTrader>().powerOnInt = powered;
        if (embryoGestationTicks is int ticks)
        {
            vat.GetComp<CompEmbryoGestation>().props = new CompProperties_EmbryoGestation
            {
                embryoGestationTicks = ticks,
            };
        }
        return vat;
    }

    public HumanEmbryo SpawnEmbryo() =>
        Track((HumanEmbryo)GenSpawn.Spawn(ThingDefOf.HumanEmbryo, Map.Center, Map));

    // Puts a new embryo in the vat and selects it, the state the vat starts gestating from.
    public HumanEmbryo LoadEmbryo(Building_GrowthVat vat)
    {
        var embryo = Track((HumanEmbryo)ThingMaker.MakeThing(ThingDefOf.HumanEmbryo));
        Expect.IsTrue(vat.innerContainer.TryAdd(embryo), "embryo added to the vat");
        vat.SelectEmbryo(embryo);
        return embryo;
    }

    private HumanEmbryo Track(HumanEmbryo embryo)
    {
        _embryos.Add(embryo);
        return embryo;
    }

    public void Dispose()
    {
        foreach (var vat in _vats)
        {
            // Emptied and stopped first so destroying the vat doesn't eject a gestating embryo
            // with a message.
            vat.innerContainer.ClearAndDestroyContents();
            vat.selectedEmbryo = null;
            vat.startTick = -1;
            if (!vat.Destroyed)
            {
                vat.Destroy();
            }
        }
        foreach (var embryo in _embryos)
        {
            if (!embryo.Destroyed)
            {
                embryo.Destroy();
            }
        }
        if (_addedCompProperties != null)
        {
            _ = ThingDefOf.GrowthVat.comps.Remove(_addedCompProperties);
        }
        Find.Storyteller.difficulty.noBabiesOrChildren = _noBabiesOrChildren;
    }
}

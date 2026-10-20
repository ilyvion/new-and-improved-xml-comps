using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class GravEngineOrbSelectionTests
{
    private static readonly GraphicData Orb = new() { drawSize = new Vector2(2f, 4f) };
    private static readonly GraphicData CooldownOrb = new() { drawSize = new Vector2(5f, 6f) };

    private static Building_GravEngine MakeEngine(params DefModExtension[] modExtensions) =>
        new() { def = new ThingDef { modExtensions = [.. modExtensions] } };

    [Test]
    public static void WithoutTheExtensionNoOrbIsSelected()
    {
        Expect.IsNull(Building_GravEngine_DrawAt_Patch.SelectOrbGraphic(null, false));
        Expect.IsNull(Building_GravEngine_DrawAt_Patch.SelectOrbGraphic(null, true));
    }

    [Test]
    public static void WithoutAnOrbGraphicNoOrbIsSelected()
    {
        var extension = new GravshipOrbExtension { cooldownOrbGraphic = CooldownOrb };

        Expect.IsNull(Building_GravEngine_DrawAt_Patch.SelectOrbGraphic(extension, false));
        Expect.IsNull(Building_GravEngine_DrawAt_Patch.SelectOrbGraphic(extension, true));
    }

    [Test]
    public static void WithOnlyAnOrbGraphicItIsUsedOnAndOffCooldown()
    {
        var extension = new GravshipOrbExtension { orbGraphic = Orb };

        Expect.ReferencesAreEqual(
            Orb,
            Building_GravEngine_DrawAt_Patch.SelectOrbGraphic(extension, false)
        );
        Expect.ReferencesAreEqual(
            Orb,
            Building_GravEngine_DrawAt_Patch.SelectOrbGraphic(extension, true)
        );
    }

    [Test]
    public static void TheCooldownOrbGraphicIsUsedOnlyOnCooldown()
    {
        var extension = new GravshipOrbExtension
        {
            orbGraphic = Orb,
            cooldownOrbGraphic = CooldownOrb,
        };

        Expect.ReferencesAreEqual(
            Orb,
            Building_GravEngine_DrawAt_Patch.SelectOrbGraphic(extension, false)
        );
        Expect.ReferencesAreEqual(
            CooldownOrb,
            Building_GravEngine_DrawAt_Patch.SelectOrbGraphic(extension, true)
        );
    }

    [Test]
    public static void WithoutTheExtensionTheOrbKeepsVanillasSize()
    {
        var size = new Vector3(3f, 1f, 3f);

        Building_GravEngine_DrawAt_Patch.UpdateVector3(MakeEngine(), ref size);

        Expect.AreEqual(new Vector3(3f, 1f, 3f), size);
    }

    [Test]
    public static void WithoutAnOrbGraphicTheOrbKeepsVanillasSize()
    {
        var size = new Vector3(3f, 1f, 3f);
        var engine = MakeEngine(new GravshipOrbExtension { cooldownOrbGraphic = CooldownOrb });

        Building_GravEngine_DrawAt_Patch.UpdateVector3(engine, ref size);

        Expect.AreEqual(new Vector3(3f, 1f, 3f), size);
    }

    // The documented behavior: the orb is drawn at orbGraphic's drawSize, also while
    // cooldownOrbGraphic is shown.
    [Test]
    public static void TheOrbIsDrawnAtTheOrbGraphicsSizeEvenWithACooldownOrb()
    {
        var size = new Vector3(3f, 1f, 3f);
        var engine = MakeEngine(
            new GravshipOrbExtension { orbGraphic = Orb, cooldownOrbGraphic = CooldownOrb }
        );

        Building_GravEngine_DrawAt_Patch.UpdateVector3(engine, ref size);

        Expect.AreEqual(new Vector3(2f, 1f, 4f), size);
    }
}

[TestFixture(TestType.Playing)]
[LoadIfOdysseyActive]
internal sealed class GravEngineGraphicTests
{
    // NIXC_CustomGravEngine's graphicData and GravshipCooldownExtension/GravshipOrbExtension
    // texture paths, and vanilla's own cooldown graphic.
    private const string BasePath = "Things/Building/GravEngine/GravEngine";
    private const string CooldownPath = "NIXC_GravEngine_Cooldown";
    private const string VanillaCooldownPath = "Things/Building/GravEngine/GravEngine_Cooldown";
    private const string OrbPath = "NIXC_RedOrb_Orbed";
    private const string CooldownOrbPath = "NIXC_BlueOrb_Orbed";

    private static Building_GravEngine MakeEngine(ThingDef def, int ticksUntilCooldownEnds)
    {
        var engine = (Building_GravEngine)ThingMaker.MakeThing(def);
        engine.cooldownCompleteTick = Find.TickManager.TicksGame + ticksUntilCooldownEnds;
        return engine;
    }

    private static Building_GravEngine MakeCustomEngine(int ticksUntilCooldownEnds) =>
        MakeEngine(DefDatabase<ThingDef>.GetNamed("NIXC_CustomGravEngine"), ticksUntilCooldownEnds);

    private static Texture OrbTexture(Building_GravEngine engine) =>
        Building_GravEngine_DrawAt_Patch.GetOrbGraphic(BaseContent.BadMat, engine).mainTexture;

    [Test]
    public static void OnCooldownTheEngineUsesTheCooldownGraphic() =>
        Expect.AreEqual(CooldownPath, MakeCustomEngine(1000).Graphic.path);

    [Test]
    public static void OffCooldownTheEngineUsesItsOwnGraphic() =>
        Expect.AreEqual(BasePath, MakeCustomEngine(-1).Graphic.path);

    [Test]
    public static void TheCooldownGraphicEndsOnTheCooldownCompleteTick() =>
        Expect.AreEqual(BasePath, MakeCustomEngine(0).Graphic.path);

    [Test]
    public static void AnEngineWithoutTheExtensionKeepsVanillasCooldownGraphic() =>
        Expect.AreEqual(VanillaCooldownPath, MakeEngine(ThingDefOf.GravEngine, 1000).Graphic.path);

    [Test]
    public static void AnExtensionWithoutACooldownGraphicKeepsTheGraphic()
    {
        var engine = new Building_GravEngine
        {
            def = new ThingDef { modExtensions = [new GravshipCooldownExtension()] },
            cooldownCompleteTick = Find.TickManager.TicksGame + 1000,
        };
        var graphic = BaseContent.BadGraphic;

        Building_GravEngine_Graphic_Patch.Postfix(engine, ref graphic);

        Expect.ReferencesAreEqual(BaseContent.BadGraphic, graphic);
    }

    [Test]
    public static void OnCooldownTheCooldownOrbIsDrawn() =>
        Expect.ReferencesAreEqual(
            ContentFinder<Texture2D>.Get(CooldownOrbPath),
            OrbTexture(MakeCustomEngine(1000))
        );

    [Test]
    public static void OffCooldownTheOrbIsDrawn() =>
        Expect.ReferencesAreEqual(
            ContentFinder<Texture2D>.Get(OrbPath),
            OrbTexture(MakeCustomEngine(-1))
        );

    [Test]
    public static void TheCooldownOrbEndsOnTheCooldownCompleteTick() =>
        Expect.ReferencesAreEqual(
            ContentFinder<Texture2D>.Get(OrbPath),
            OrbTexture(MakeCustomEngine(0))
        );

    [Test]
    public static void AnEngineWithoutTheExtensionKeepsVanillasOrb() =>
        Expect.ReferencesAreEqual(
            BaseContent.BadMat,
            Building_GravEngine_DrawAt_Patch.GetOrbGraphic(
                BaseContent.BadMat,
                MakeEngine(ThingDefOf.GravEngine, 1000)
            )
        );
}

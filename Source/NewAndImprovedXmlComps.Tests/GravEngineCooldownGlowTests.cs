using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class GravEngineCooldownGlowOverrideChangeTests
{
    private const float Configured = 40f;
    private const float Other = 12f;

    private static (bool Changed, float? Override) Decide(
        bool onCooldown,
        float? configured,
        float? current
    )
    {
        var changed = Building_GravEngine_Tick_Patch.TryGetOverrideChange(
            onCooldown,
            configured,
            current,
            out var result
        );
        return (changed, result);
    }

    [Test]
    public static void NothingConfiguredNeverChangesTheOverride()
    {
        Expect.AreEqual((false, null), Decide(true, null, null));
        Expect.AreEqual((false, Other), Decide(true, null, Other));
        Expect.AreEqual((false, null), Decide(false, null, null));
        Expect.AreEqual((false, Other), Decide(false, null, Other));
    }

    [Test]
    public static void OnCooldownWithoutAnOverrideAppliesTheConfiguredValue() =>
        Expect.AreEqual((true, Configured), Decide(true, Configured, null));

    [Test]
    public static void OnCooldownWithTheConfiguredOverrideAlreadyAppliedChangesNothing() =>
        Expect.AreEqual((false, Configured), Decide(true, Configured, Configured));

    [Test]
    public static void OnCooldownAnotherOverrideIsKept() =>
        Expect.AreEqual((false, Other), Decide(true, Configured, Other));

    [Test]
    public static void OffCooldownTheConfiguredOverrideIsRemoved() =>
        Expect.AreEqual((true, null), Decide(false, Configured, Configured));

    [Test]
    public static void OffCooldownAnotherOverrideIsKept() =>
        Expect.AreEqual((false, Other), Decide(false, Configured, Other));

    [Test]
    public static void OffCooldownWithoutAnOverrideChangesNothing() =>
        Expect.AreEqual((false, null), Decide(false, Configured, null));

    [Test]
    public static void ColorOverridesAreComparedByValue()
    {
        var changed = Building_GravEngine_Tick_Patch.TryGetOverrideChange<ColorInt>(
            false,
            new ColorInt(255, 218, 0, 0),
            new ColorInt(255, 218, 0, 0),
            out var result
        );

        Expect.IsTrue(changed);
        Expect.IsFalse(result.HasValue);
    }
}

[TestFixture(TestType.Playing)]
[LoadIfOdysseyActive]
internal sealed class GravEngineCooldownGlowTests
{
    // NIXC_CustomGravEngine's CompProperties_Glower and GravshipCooldownExtension values.
    private const float NormalRadius = 8f;
    private const float CooldownRadius = 40f;
    private static readonly ColorInt NormalColor = new(0, 218, 255, 0);
    private static readonly ColorInt CooldownColor = new(255, 218, 0, 0);
    private static readonly ColorInt PlayerColor = new(255, 0, 0, 0);

    // Farther from the engine than the normal glow radius reaches, within the cooldown radius.
    private static readonly IntVec3 FarCellOffset = new(20, 0, 0);

    private sealed class EngineScene : IDisposable
    {
        public Building_GravEngine Engine { get; }
        public CompGlower Glower { get; }

        public EngineScene(Map map)
        {
            var def = DefDatabase<ThingDef>.GetNamed("NIXC_CustomGravEngine");
            Expect.IsEmpty(
                map.listerThings.ThingsOfDef(def),
                "grav engines left behind by an earlier test"
            );

            Engine = (Building_GravEngine)
                GenSpawn.Spawn(ThingMaker.MakeThing(def), map.Center, map);
            Glower = Engine.GetComp<CompGlower>();
        }

        public void SetOnCooldown(bool onCooldown) =>
            Engine.cooldownCompleteTick = Find.TickManager.TicksGame + (onCooldown ? 1000 : -1);

        public void Tick() => Building_GravEngine_Tick_Patch.Postfix(Engine);

        public bool FarCellIsLit()
        {
            var glowGrid = Engine.Map.glowGrid;
            glowGrid.GlowGridUpdate_First();
            var glow = glowGrid.VisualGlowAt(Engine.Position + FarCellOffset);
            return glow.r > 0 || glow.g > 0 || glow.b > 0;
        }

        public void Dispose()
        {
            if (!Engine.Destroyed)
            {
                Engine.Destroy();
            }
        }
    }

    [Test]
    public static void OnCooldownTheGlowUsesTheCooldownRadiusAndColor()
    {
        using var scene = new EngineScene(Find.CurrentMap);
        scene.SetOnCooldown(true);

        scene.Tick();

        Expect.AreApproximatelyEqual(CooldownRadius, scene.Glower.GlowRadius);
        Expect.AreEqual(CooldownColor, scene.Glower.GlowColor);
        Expect.IsTrue(scene.FarCellIsLit(), "cooldown radius reaches the glow grid");
    }

    [Test]
    public static void AfterTheCooldownTheGlowGoesBackToNormal()
    {
        using var scene = new EngineScene(Find.CurrentMap);
        scene.SetOnCooldown(true);
        scene.Tick();

        scene.SetOnCooldown(false);
        scene.Tick();

        Expect.AreApproximatelyEqual(NormalRadius, scene.Glower.GlowRadius);
        Expect.AreEqual(NormalColor, scene.Glower.GlowColor);
        Expect.IsFalse(scene.Glower.HasGlowColorOverride);
        Expect.IsFalse(scene.FarCellIsLit(), "normal radius reaches the glow grid");
    }

    [Test]
    public static void TheCooldownEndsOnTheCooldownCompleteTick()
    {
        using var scene = new EngineScene(Find.CurrentMap);
        scene.SetOnCooldown(true);
        scene.Tick();

        scene.Engine.cooldownCompleteTick = Find.TickManager.TicksGame;
        scene.Tick();

        Expect.AreApproximatelyEqual(NormalRadius, scene.Glower.GlowRadius);
        Expect.AreEqual(NormalColor, scene.Glower.GlowColor);
    }

    // The glower's color override is saved and its radius override isn't, so this is the state
    // of an engine loaded from a save made during its cooldown.
    [Test]
    public static void TheCooldownRadiusReachesTheGlowGridWhenTheColorIsAlreadyApplied()
    {
        using var scene = new EngineScene(Find.CurrentMap);
        scene.Glower.GlowColor = CooldownColor;
        scene.SetOnCooldown(true);

        scene.Tick();

        Expect.AreApproximatelyEqual(CooldownRadius, scene.Glower.GlowRadius);
        Expect.IsTrue(scene.FarCellIsLit(), "cooldown radius reaches the glow grid");
    }

    [Test]
    public static void AColorSetByThePlayerIsKeptDuringAndAfterTheCooldown()
    {
        using var scene = new EngineScene(Find.CurrentMap);
        scene.Glower.GlowColor = PlayerColor;

        scene.SetOnCooldown(true);
        scene.Tick();
        Expect.AreEqual(PlayerColor, scene.Glower.GlowColor, "during the cooldown");

        scene.SetOnCooldown(false);
        scene.Tick();
        Expect.AreEqual(PlayerColor, scene.Glower.GlowColor, "after the cooldown");
    }

    [Test]
    public static void AnEngineWithoutTheExtensionKeepsItsGlow()
    {
        var map = Find.CurrentMap;
        var engine = (Building_GravEngine)
            GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.GravEngine), map.Center, map);
        try
        {
            var glower = engine.GetComp<CompGlower>();
            var radius = glower.GlowRadius;
            var color = glower.GlowColor;
            engine.cooldownCompleteTick = Find.TickManager.TicksGame + 1000;

            Building_GravEngine_Tick_Patch.Postfix(engine);

            Expect.AreApproximatelyEqual(radius, glower.GlowRadius);
            Expect.AreEqual(color, glower.GlowColor);
        }
        finally
        {
            engine.Destroy();
        }
    }
}

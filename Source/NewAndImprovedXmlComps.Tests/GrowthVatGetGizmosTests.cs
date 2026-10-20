using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

// The idle vat's "Implant embryo" gizmo tells the player how long gestation takes in that vat.
[TestFixture(TestType.Playing)]
[LoadIfBiotechActive]
internal sealed class GrowthVatGetGizmosTests
{
    private static string? ImplantEmbryoDescription(int? embryoGestationTicks)
    {
        using var scene = new GrowthVatScene(Find.CurrentMap, embryoGestationTicks);
        var vat = scene.SpawnVat(IntVec3.Zero);

        var label = "ImplantEmbryo".Translate() + "...";
        var gizmo = vat.GetGizmos()
            .ToList()
            .OfType<Command_Action>()
            .FirstOrDefault(g => g.defaultLabel == label);
        Expect.IsNotNull(gizmo, "implant embryo gizmo");
        return gizmo?.defaultDesc;
    }

    private static bool Mentions(string? description, int ticks) =>
        description?.Contains(ticks.ToStringTicksToPeriod(), StringComparison.Ordinal) ?? false;

    [Test]
    public static void WithoutTheCompTheDescriptionUsesTheVanillaDuration() =>
        Expect.IsTrue(
            Mentions(ImplantEmbryoDescription(null), Building_GrowthVat.EmbryoGestationTicks)
        );

    [Test]
    public static void WithTheCompTheDescriptionUsesTheCustomDuration()
    {
        var description = ImplantEmbryoDescription(GenDate.TicksPerDay);

        Expect.IsTrue(Mentions(description, GenDate.TicksPerDay));
        Expect.IsFalse(Mentions(description, Building_GrowthVat.EmbryoGestationTicks));
    }
}

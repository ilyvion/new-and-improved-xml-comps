using DevTools.Testing;

namespace NewAndImprovedXmlComps.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class CompImprovedSpawnerIsActiveTests
{
    // Regression guard (0.1.1): with both requiresPower and requiresFuel, the spawner needs power
    // and fuel; it used to run with either one.
    [TestCase(true, true, true, false, ExpectedResult = false)]
    [TestCase(true, true, false, true, ExpectedResult = false)]
    [TestCase(true, true, true, true, ExpectedResult = true)]
    [TestCase(true, true, false, false, ExpectedResult = false)]
    [TestCase(false, false, false, false, ExpectedResult = true)]
    [TestCase(false, false, true, false, ExpectedResult = true)]
    [TestCase(false, false, false, true, ExpectedResult = true)]
    [TestCase(false, false, true, true, ExpectedResult = true)]
    [TestCase(true, false, false, false, ExpectedResult = false)]
    [TestCase(true, false, false, true, ExpectedResult = false)]
    [TestCase(true, false, true, false, ExpectedResult = true)]
    [TestCase(true, false, true, true, ExpectedResult = true)]
    [TestCase(false, true, false, false, ExpectedResult = false)]
    [TestCase(false, true, true, false, ExpectedResult = false)]
    [TestCase(false, true, false, true, ExpectedResult = true)]
    [TestCase(false, true, true, true, ExpectedResult = true)]
    public static bool IsActiveOnlyWhenEveryRequirementIsMet(
        bool requiresPower,
        bool requiresFuel,
        bool powerOn,
        bool hasFuel
    ) => CompImprovedSpawner.IsActive(requiresPower, requiresFuel, () => powerOn, () => hasFuel);

    private sealed class TestableSpawner : CompImprovedSpawner
    {
        public bool IsActivePublic() => IsActive();
    }

    private static TestableSpawner MakeSpawnerWithoutOtherComps(
        bool requiresPower,
        bool requiresFuel
    ) =>
        new()
        {
            parent = new ThingWithComps(),
            props = new CompProperties_ImprovedSpawner
            {
                requiresPower = requiresPower,
                requiresFuel = requiresFuel,
            },
        };

    [Test]
    public static void RequiringPowerWithoutAPowerCompIsInactive() =>
        Expect.IsFalse(MakeSpawnerWithoutOtherComps(true, false).IsActivePublic());

    [Test]
    public static void RequiringFuelWithoutARefuelableCompIsInactive() =>
        Expect.IsFalse(MakeSpawnerWithoutOtherComps(false, true).IsActivePublic());

    [Test]
    public static void RequiringNeitherWithoutAnyCompsIsActive() =>
        Expect.IsTrue(MakeSpawnerWithoutOtherComps(false, false).IsActivePublic());
}

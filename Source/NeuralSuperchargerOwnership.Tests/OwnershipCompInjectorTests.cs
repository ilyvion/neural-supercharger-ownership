using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// Which race defs get the ownership comp, so modded humanlike races can own a supercharger.
[TestFixture(TestType.MainMenu)]
internal sealed class OwnershipCompInjectorTests
{
    private static ThingDef MakeRaceDef(Intelligence intelligence, List<CompProperties>? comps) =>
        new()
        {
            defName = "NeuralSuperchargerOwnership_TestRace",
            race = new RaceProperties { intelligence = intelligence },
            comps = comps!,
        };

    private static int OwnershipCompCount(ThingDef def) =>
        def.comps.Count(c => c.compClass == typeof(CompNeuralSuperchargerOwnership));

    [Test]
    public static void EveryLoadedHumanlikeRaceHasTheComp()
    {
        foreach (var def in DefDatabase<ThingDef>.AllDefs.Where(d => d.race?.Humanlike == true))
        {
            Expect.AreEqual(1, OwnershipCompCount(def), def.defName);
        }
    }

    [Test]
    public static void AHumanlikeRaceGetsTheComp()
    {
        var def = MakeRaceDef(Intelligence.Humanlike, []);

        OwnershipCompInjector.AddOwnershipComp([def]);

        Expect.AreEqual(1, OwnershipCompCount(def));
    }

    [Test]
    public static void AHumanlikeRaceWithoutACompListGetsTheComp()
    {
        var def = MakeRaceDef(Intelligence.Humanlike, null);

        OwnershipCompInjector.AddOwnershipComp([def]);

        Expect.AreEqual(1, OwnershipCompCount(def));
    }

    [Test]
    public static void ARaceThatAlreadyHasTheCompDoesNotGetASecondOne()
    {
        var def = MakeRaceDef(Intelligence.Humanlike, []);

        OwnershipCompInjector.AddOwnershipComp([def]);
        OwnershipCompInjector.AddOwnershipComp([def]);

        Expect.AreEqual(1, OwnershipCompCount(def));
    }

    [Test]
    public static void AnAnimalRaceDoesNotGetTheComp()
    {
        var def = MakeRaceDef(Intelligence.Animal, []);

        OwnershipCompInjector.AddOwnershipComp([def]);

        Expect.AreEqual(0, OwnershipCompCount(def));
    }

    [Test]
    public static void ANonRaceDefDoesNotGetTheComp()
    {
        var def = new ThingDef { defName = "NeuralSuperchargerOwnership_TestThing", comps = [] };

        OwnershipCompInjector.AddOwnershipComp([def]);

        Expect.AreEqual(0, OwnershipCompCount(def));
    }
}

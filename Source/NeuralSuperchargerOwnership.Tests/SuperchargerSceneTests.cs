using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// The scene's things are what every ownership test builds on, so they must come out as the tests
// expect: patched with the mod's comps, player-owned and with nobody assigned yet.
[TestFixture(TestType.Playing)]
internal sealed class SuperchargerSceneTests
{
    [Test]
    public static void MakesAnUnownedChargedPlayerSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);

        var supercharger = scene.MakeSupercharger();

        Expect.IsTrue(supercharger.Spawned, "Not spawned");
        Expect.AreEqual(Faction.OfPlayer, supercharger.Faction);
        Expect.IsTrue(supercharger.GetComp<CompNeuralSupercharger>().Charged, "Not charged");
        Expect.IsEmpty(supercharger.AssignedOwnership().AssignedPawnsForReading);
    }

    [Test]
    public static void MakesAnUnspawnedUnchargedSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);

        var supercharger = scene.MakeSupercharger(spawn: false, charged: false);

        Expect.IsFalse(supercharger.Spawned, "Spawned");
        Expect.IsFalse(supercharger.GetComp<CompNeuralSupercharger>().Charged, "Charged");
    }

    [Test]
    public static void MakesSeveralSpawnedSuperchargers()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);

        var first = scene.MakeSupercharger();
        var second = scene.MakeSupercharger();

        Expect.IsTrue(first.Spawned && second.Spawned, "Not both spawned");
    }

    [Test]
    public static void MakesAColonistOwningNothing()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        _ = scene.MakeSupercharger();

        var colonist = scene.MakeColonist();

        Expect.IsTrue(colonist.Spawned, "Not spawned");
        Expect.IsTrue(colonist.IsColonist, "Not a colonist");
        scene.ExpectConsistent(colonist, null);
    }
}

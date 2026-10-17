using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// A destroyed supercharger frees its owner; one that is only despawned, as minifying does, keeps it.
[TestFixture(TestType.Playing)]
internal sealed class SuperchargerDestroyTests
{
    [SetUp]
    public static void ClearMessages() => Messages.Clear();

    [Test]
    public static void DeconstructingFreesTheOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(spawn: false);
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        supercharger.Destroy(DestroyMode.Deconstruct);

        scene.ExpectConsistent(pawn, null);
        Expect.IsEmpty(supercharger.AssignedOwnership().AssignedPawnsForReading);
        ExpectOnlyMessage("MessageBedLostAssignment", pawn);
    }

    [Test]
    public static void BreakingFreesTheOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(spawn: false);
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        supercharger.Destroy(DestroyMode.KillFinalize);

        scene.ExpectConsistent(pawn, null);
        Expect.IsEmpty(supercharger.AssignedOwnership().AssignedPawnsForReading);
        ExpectOnlyMessage("MessageBedDestroyed", pawn);
    }

    [Test]
    public static void VanishingFreesTheOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        supercharger.Destroy(DestroyMode.Vanish);

        scene.ExpectConsistent(pawn, null);
        Expect.IsEmpty(supercharger.AssignedOwnership().AssignedPawnsForReading);
        Expect.IsFalse(supercharger.AssignedOwnership().AssignedAnything(pawn));
        ExpectOnlyMessage("MessageBedLostAssignment", pawn);
    }

    [Test]
    public static void VanishingWhileAwaitingReinstallFreesTheOwnerQuietly()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);
        var blueprint = GenConstruct.PlaceBlueprintForReinstall(
            supercharger,
            new IntVec3(30, 0, 30),
            scene.Map,
            Rot4.North,
            Faction.OfPlayer
        );

        supercharger.Destroy(DestroyMode.Vanish);

        scene.ExpectConsistent(pawn, null);
        Expect.IsTrue(blueprint.Destroyed, "The reinstall blueprint wasn't cancelled");
        ExpectOnlyMessage(null, pawn);
    }

    [Test]
    public static void DestroyingWithoutAnOwnerSendsNoMessage()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var deconstructed = scene.MakeSupercharger(spawn: false);
        var vanished = scene.MakeSupercharger();

        deconstructed.Destroy(DestroyMode.Deconstruct);
        vanished.Destroy(DestroyMode.Vanish);

        Expect.IsEmpty(Messages.liveMessages);
    }

    [Test]
    public static void MinifyingAndReinstallingKeepsTheOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);
        var cell = supercharger.Position;

        var minified = supercharger.MakeMinified();

        scene.ExpectConsistent(pawn, supercharger);

        // Installing takes the supercharger out of its minified thing, as Blueprint_Install does.
        minified.InnerThing = null;
        minified.Destroy();
        _ = GenSpawn.Spawn(supercharger, cell, scene.Map, Rot4.North);

        Expect.IsTrue(supercharger.Spawned, "Not reinstalled");
        scene.ExpectConsistent(pawn, supercharger);
        ExpectOnlyMessage(null, pawn);
    }

    // A null key means no message at all.
    private static void ExpectOnlyMessage(string? key, Pawn pawn)
    {
        if (key == null)
        {
            Expect.IsEmpty(Messages.liveMessages);
            return;
        }

        var text = ((string)key.Translate(ThingDefOf.NeuralSupercharger, pawn)).CapitalizeFirst();
        Expect.AreEqual(1, Messages.liveMessages.Count);
        Expect.Any(Messages.liveMessages, message => message.text == text, $"No '{text}'");
    }
}

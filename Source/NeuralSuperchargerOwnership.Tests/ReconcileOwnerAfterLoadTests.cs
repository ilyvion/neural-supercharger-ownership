using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// After loading, a supercharger's owner is made to own it, which is how saves from 0.1.0 (where
// only the supercharger recorded its owner) get the pawn's side of ownership.
[TestFixture(TestType.Playing)]
internal sealed class ReconcileOwnerAfterLoadTests
{
    [Test]
    [WarningsAllowed("upgrading from version 0.1.0")]
    public static void OwnerWhoDoesNotOwnTheSuperchargerIsGivenIt()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().ForceAddPawn(pawn);

        supercharger.AssignedOwnership().ReconcileOwnerAfterLoad();

        scene.ExpectConsistent(pawn, supercharger);
    }

    [Test]
    public static void ConsistentOwnerIsLeftAlone()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        supercharger.AssignedOwnership().ReconcileOwnerAfterLoad();

        scene.ExpectConsistent(pawn, supercharger);
    }

    [Test]
    public static void UnownedSuperchargerIsLeftAlone()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();

        supercharger.AssignedOwnership().ReconcileOwnerAfterLoad();

        Expect.IsEmpty(supercharger.AssignedOwnership().AssignedPawnsForReading);
    }

    [Test]
    [WarningsAllowed("upgrading from version 0.1.0")]
    public static void OwnerOfAnotherSuperchargerGivesThatOneUp()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var other = scene.MakeSupercharger();
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        other.AssignedOwnership().TryAssignPawn(pawn);
        supercharger.AssignedOwnership().ForceAddPawn(pawn);

        supercharger.AssignedOwnership().ReconcileOwnerAfterLoad();

        scene.ExpectConsistent(pawn, supercharger);
        Expect.IsEmpty(other.AssignedOwnership().AssignedPawnsForReading);
    }
}

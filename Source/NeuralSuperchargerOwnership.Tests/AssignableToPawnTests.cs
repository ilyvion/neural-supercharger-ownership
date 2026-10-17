using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// The vanilla assignment API, which the assign-owner dialog uses to change ownership.
[TestFixture(TestType.Playing)]
internal sealed class AssignableToPawnTests
{
    [Test]
    public static void OwningAnotherSuperchargerCountsAsAssigned()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var other = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        other.AssignedOwnership().TryAssignPawn(pawn);

        Expect.IsTrue(supercharger.AssignedOwnership().AssignedAnything(pawn));
    }

    [Test]
    public static void OwningNothingDoesNotCountAsAssigned()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();

        Expect.IsFalse(supercharger.AssignedOwnership().AssignedAnything(pawn));
    }

    [Test]
    public static void AssignedAnythingRejectsNull()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();

        _ = Expect.Throws<ArgumentNullException>(() =>
            supercharger.AssignedOwnership().AssignedAnything(null!)
        );
    }

    [Test]
    public static void AssigningAnUnownedSuperchargerOwnsIt()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();

        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        scene.ExpectConsistent(pawn, supercharger);
    }

    [Test]
    public static void AssigningTheOwnerAgainChangesNothing()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        scene.ExpectConsistent(pawn, supercharger);
    }

    [Test]
    public static void AssigningAnotherSuperchargerGivesUpTheFirst()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var first = scene.MakeSupercharger();
        var second = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        first.AssignedOwnership().TryAssignPawn(pawn);

        second.AssignedOwnership().TryAssignPawn(pawn);

        scene.ExpectConsistent(pawn, second);
        Expect.IsEmpty(first.AssignedOwnership().AssignedPawnsForReading);
    }

    [Test]
    public static void AssigningSomeoneElsesSuperchargerTakesItFromThem()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var previousOwner = scene.MakeColonist();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(previousOwner);

        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        scene.ExpectConsistent(pawn, supercharger);
        scene.ExpectConsistent(previousOwner, null);
    }

    [Test]
    public static void AssigningDropsThePawnFromTheUninstalledOwners()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().uninstalledAssignedPawns.Add(pawn);

        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        scene.ExpectConsistent(pawn, supercharger);
        Expect.IsEmpty(supercharger.AssignedOwnership().uninstalledAssignedPawns);
    }

    [Test]
    public static void UnassigningTheOwnerFreesTheSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        supercharger.AssignedOwnership().TryUnassignPawn(pawn);

        scene.ExpectConsistent(pawn, null);
        Expect.IsEmpty(supercharger.AssignedOwnership().uninstalledAssignedPawns);
    }

    [Test]
    public static void UnassigningForUninstallRemembersTheOwnerOnce()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        supercharger.AssignedOwnership().TryUnassignPawn(pawn, uninstall: true);
        supercharger.AssignedOwnership().TryUnassignPawn(pawn, uninstall: true);

        scene.ExpectConsistent(pawn, null);
        var uninstalled = supercharger.AssignedOwnership().uninstalledAssignedPawns;
        Expect.AreEqual(1, uninstalled.Count);
        Expect.IsTrue(uninstalled.Contains(pawn), "The owner isn't remembered");
    }

    // Unassigning a pawn from a supercharger it doesn't own once unclaimed the supercharger it
    // did own, and with uninstall set, remembered the pawn as an uninstalled owner of this one.
    [Test]
    public static void UnassigningFromASuperchargerThePawnDoesNotOwnChangesNothing()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var owned = scene.MakeSupercharger();
        var other = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        owned.AssignedOwnership().TryAssignPawn(pawn);

        other.AssignedOwnership().TryUnassignPawn(pawn, uninstall: true);

        scene.ExpectConsistent(pawn, owned);
        Expect.IsEmpty(other.AssignedOwnership().uninstalledAssignedPawns);
    }

    [Test]
    public static void NullCannotBeRestoredAsUninstalledOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();

        Expect.IsFalse(supercharger.AssignedOwnership().CanSetUninstallAssignedPawn(null!));
    }

    [Test]
    public static void PawnOwningASuperchargerCannotBeRestoredAsUninstalledOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var other = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        other.AssignedOwnership().TryAssignPawn(pawn);

        Expect.IsFalse(supercharger.AssignedOwnership().CanSetUninstallAssignedPawn(pawn));
    }

    [Test]
    public static void FreeColonistCanBeRestoredAsUninstalledOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();

        Expect.IsTrue(supercharger.AssignedOwnership().CanSetUninstallAssignedPawn(pawn));
    }

    [Test]
    public static void FreePrisonerCanBeRestoredAsUninstalledOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakePrisoner();

        Expect.IsTrue(pawn.IsPrisonerOfColony, "Not a prisoner of the colony");
        Expect.IsTrue(supercharger.AssignedOwnership().CanSetUninstallAssignedPawn(pawn));
    }

    [Test]
    public static void StrangerCannotBeRestoredAsUninstalledOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeStranger();

        Expect.IsFalse(supercharger.AssignedOwnership().CanSetUninstallAssignedPawn(pawn));
    }
}

using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// Claiming and unclaiming keep the pawn's and the supercharger's sides of ownership in sync. They
// guard the 0.2.0 ownership rewrite.
[TestFixture(TestType.Playing)]
internal sealed class ClaimNeuralSuperchargerTests
{
    [Test]
    public static void ClaimingAnUnownedSuperchargerOwnsIt()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();

        Expect.IsTrue(pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(supercharger));

        scene.ExpectConsistent(pawn, supercharger);
    }

    [Test]
    public static void ClaimingAnOwnedSuperchargerAgainChangesNothing()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        _ = pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(supercharger);

        Expect.IsFalse(pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(supercharger));

        scene.ExpectConsistent(pawn, supercharger);
    }

    [Test]
    public static void ClaimingAnotherSuperchargerGivesUpTheFirst()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var first = scene.MakeSupercharger();
        var second = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        _ = pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(first);

        Expect.IsTrue(pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(second));

        scene.ExpectConsistent(pawn, second);
        Expect.IsEmpty(first.AssignedOwnership().AssignedPawnsForReading);
    }

    [Test]
    public static void ClaimingSomeoneElsesSuperchargerTakesItFromThem()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var previousOwner = scene.MakeColonist();
        var pawn = scene.MakeColonist();
        _ = previousOwner.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(supercharger);

        Expect.IsTrue(pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(supercharger));

        scene.ExpectConsistent(pawn, supercharger);
        scene.ExpectConsistent(previousOwner, null);
    }

    [Test]
    public static void ClaimingSomeoneElsesSuperchargerFreesBothOldSuperchargers()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var own = scene.MakeSupercharger();
        var other = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        var otherOwner = scene.MakeColonist();
        _ = pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(own);
        _ = otherOwner.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(other);

        Expect.IsTrue(pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(other));

        scene.ExpectConsistent(pawn, other);
        scene.ExpectConsistent(otherOwner, null);
        Expect.IsEmpty(own.AssignedOwnership().AssignedPawnsForReading);
    }

    [Test]
    public static void UnclaimingWithNothingOwnedDoesNothing()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        _ = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();

        Expect.IsFalse(pawn.NeuralSuperchargerOwnership().UnclaimNeuralSupercharger());

        scene.ExpectConsistent(pawn, null);
    }

    [Test]
    public static void UnclaimingFreesTheSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        _ = pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(supercharger);

        Expect.IsTrue(pawn.NeuralSuperchargerOwnership().UnclaimNeuralSupercharger());

        scene.ExpectConsistent(pawn, null);
        Expect.IsEmpty(supercharger.AssignedOwnership().AssignedPawnsForReading);
    }

    [Test]
    public static void UnclaimingDropsThePawnFromTheUninstalledOwners()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        _ = pawn.NeuralSuperchargerOwnership().ClaimNeuralSupercharger(supercharger);
        supercharger.AssignedOwnership().uninstalledAssignedPawns.Add(pawn);

        _ = pawn.NeuralSuperchargerOwnership().UnclaimNeuralSupercharger();

        Expect.IsEmpty(supercharger.AssignedOwnership().uninstalledAssignedPawns);
    }
}

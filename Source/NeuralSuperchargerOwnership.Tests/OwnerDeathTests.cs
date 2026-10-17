using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// A pawn that dies or is otherwise destroyed gives up its supercharger.
[TestFixture(TestType.Playing)]
internal sealed class OwnerDeathTests
{
    [Test]
    public static void DyingFreesTheSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        SuperchargerScene.Kill(pawn);

        Expect.IsTrue(pawn.Dead, "Not dead");
        Expect.IsEmpty(supercharger.AssignedOwnership().AssignedPawnsForReading);
        Expect.IsFalse(supercharger.AssignedOwnership().AssignedAnything(pawn));
        scene.ExpectConsistent(pawn, null);
    }

    [Test]
    public static void VanishingFreesTheSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        pawn.Destroy(DestroyMode.Vanish);

        Expect.IsEmpty(supercharger.AssignedOwnership().AssignedPawnsForReading);
        Expect.IsFalse(supercharger.AssignedOwnership().AssignedAnything(pawn));
        scene.ExpectConsistent(pawn, null);
    }

    [Test]
    public static void DyingWithoutASuperchargerIsHarmless()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        _ = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();

        SuperchargerScene.Kill(pawn);

        Expect.IsTrue(pawn.Dead, "Not dead");
        scene.ExpectConsistent(pawn, null);
    }

    [Test]
    public static void SuperchargerOfTheDeadCanBeAssignedAgain()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var dead = scene.MakeColonist();
        var pawn = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(dead);
        SuperchargerScene.Kill(dead);

        supercharger.AssignedOwnership().TryAssignPawn(pawn);

        scene.ExpectConsistent(pawn, supercharger);
        scene.ExpectConsistent(dead, null);
    }
}

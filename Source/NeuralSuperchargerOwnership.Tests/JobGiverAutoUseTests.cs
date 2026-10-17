using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// Pawns only pick a supercharger to use on their own if it's unowned or theirs.
[TestFixture(TestType.Playing)]
internal sealed class JobGiverAutoUseTests
{
    private static Thing? ClosestSupercharger(Pawn pawn) =>
        new JobGiver_GetNeuralSupercharge().ClosestSupercharger(pawn);

    [Test]
    public static void AnyoneMayAutoUseAnUnownedSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var first = scene.MakeColonist();
        var second = scene.MakeColonist();

        Expect.ReferencesAreEqual(supercharger, ClosestSupercharger(first));
        Expect.ReferencesAreEqual(supercharger, ClosestSupercharger(second));
    }

    [Test]
    public static void OnlyTheOwnerMayAutoUseAnOwnedSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var owner = scene.MakeColonist();
        var other = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        Expect.ReferencesAreEqual(supercharger, ClosestSupercharger(owner));
        Expect.IsNull(ClosestSupercharger(other));
    }

    [Test]
    public static void NonOwnerSkipsACloserOwnedSuperchargerForAnUnownedOne()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var owned = scene.MakeSupercharger();
        var unowned = scene.MakeSupercharger();
        var owner = scene.MakeColonist();
        var other = scene.MakeColonist(owned.InteractionCell);

        Expect.ReferencesAreEqual(owned, ClosestSupercharger(other), "Owned isn't the closest");

        owned.AssignedOwnership().TryAssignPawn(owner);

        Expect.ReferencesAreEqual(unowned, ClosestSupercharger(other));
    }

    [Test]
    public static void OwnerMayNotAutoUseAnUnchargedSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(charged: false);
        var owner = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        Expect.IsNull(ClosestSupercharger(owner));
    }
}

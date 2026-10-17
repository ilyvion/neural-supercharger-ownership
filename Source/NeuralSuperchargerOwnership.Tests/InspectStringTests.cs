using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// The owner line in a supercharger's inspect pane.
[TestFixture(TestType.Playing)]
internal sealed class InspectStringTests
{
    private static string InspectString(Building supercharger) =>
        supercharger.AssignedOwnership().CompInspectStringExtra();

    private static string OwnerLine(string owner) => "Owner".Translate() + ": " + owner;

    [Test]
    public static void UnownedSuperchargerShowsNobody()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(spawn: false);

        Expect.AreEqual(OwnerLine("Nobody".Translate()), InspectString(supercharger));
    }

    [Test]
    public static void OwnedSuperchargerShowsTheOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(spawn: false);
        var owner = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        Expect.AreEqual(OwnerLine(owner.Label), InspectString(supercharger));
    }

    [Test]
    public static void NonPlayerSuperchargerWithANonPlayerOwnerShowsNothing()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(spawn: false);
        supercharger.SetFaction(null);
        var owner = scene.MakeStranger();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        Expect.AreEqual(string.Empty, InspectString(supercharger));
    }

    [Test]
    public static void NonPlayerSuperchargerOwnedByAColonistShowsTheOwner()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(spawn: false);
        supercharger.SetFaction(null);
        var owner = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        Expect.AreEqual(OwnerLine(owner.Label), InspectString(supercharger));
    }
}

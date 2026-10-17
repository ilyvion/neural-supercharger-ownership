using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// A supercharger owned by someone else offers only a disabled option in the right-click menu.
[TestFixture(TestType.Playing)]
internal sealed class FloatMenuOptionsTests
{
    private static List<FloatMenuOption> Options(Building supercharger, Pawn pawn) =>
        [.. supercharger.GetComp<CompNeuralSupercharger>().CompFloatMenuOptions(pawn)];

    private static string JobString(Building supercharger) =>
        (
            (CompProperties_NeuralSupercharger)supercharger.GetComp<CompNeuralSupercharger>().props
        ).jobString;

    private static string Annotated(Building supercharger, string key) =>
        JobString(supercharger) + " (" + key.Translate() + ")";

    [Test]
    public static void AnyoneIsOfferedAnUnownedSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var pawn = scene.MakeColonist();

        ExpectOnlyOption(Options(supercharger, pawn), JobString(supercharger), disabled: false);
    }

    [Test]
    public static void OwnerIsOfferedTheirSupercharger()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var owner = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        ExpectOnlyOption(Options(supercharger, owner), JobString(supercharger), disabled: false);
    }

    [Test]
    public static void NonOwnerIsToldTheSuperchargerIsOwned()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger();
        var owner = scene.MakeColonist();
        var other = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        ExpectOnlyOption(
            Options(supercharger, other),
            Annotated(supercharger, "NeuralSuperchargerOwnership.OwnedBySomeoneElse"),
            disabled: true
        );
    }

    [Test]
    public static void NonOwnerIsToldAnUnchargedSuperchargerIsOwned()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(charged: false);
        var owner = scene.MakeColonist();
        var other = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        ExpectOnlyOption(
            Options(supercharger, other),
            Annotated(supercharger, "NeuralSuperchargerOwnership.OwnedBySomeoneElse"),
            disabled: true
        );
    }

    [Test]
    public static void OwnerIsToldTheirUnchargedSuperchargerIsNotReady()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(charged: false);
        var owner = scene.MakeColonist();
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        ExpectOnlyOption(
            Options(supercharger, owner),
            Annotated(supercharger, "NeuralSuperchargeNotReady"),
            disabled: true
        );
    }

    private static void ExpectOnlyOption(List<FloatMenuOption> options, string label, bool disabled)
    {
        Expect.AreEqual(1, options.Count);
        if (options.Count != 1)
        {
            return;
        }

        Expect.AreEqual(label, options[0].Label);
        Expect.AreEqual(disabled, options[0].Disabled);
    }
}

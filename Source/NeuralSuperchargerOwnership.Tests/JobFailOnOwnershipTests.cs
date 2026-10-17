using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;
using Verse.AI;

namespace NeuralSuperchargerOwnership.Tests;

// A pawn already on its way to use a supercharger gives up as soon as someone else owns it.
[TestFixture(TestType.Playing)]
internal sealed class JobFailOnOwnershipTests
{
    // Walking to the supercharger aside, using it takes 0.55 + 1.5 seconds, so a job that ends
    // within this many ticks was failed rather than finished.
    private const int FailTicks = 60;

    private static void OrderSupercharge(Pawn pawn, Building supercharger) =>
        Expect.IsTrue(
            pawn.jobs.TryTakeOrderedJob(
                JobMaker.MakeJob(JobDefOf.GetNeuralSupercharge, supercharger),
                JobTag.Misc
            ),
            $"{pawn} refused the job"
        );

    // Jobs are pooled, so an ended job's object can come back as the pawn's next job; compare
    // what the job is rather than which object it is.
    private static bool IsGoingToUse(Pawn pawn, Building supercharger) =>
        pawn.CurJob is { } job
        && job.def == JobDefOf.GetNeuralSupercharge
        && job.targetA.Thing == supercharger;

    private static bool IsSupercharged(Pawn pawn) =>
        pawn.health.hediffSet.HasHediff(HediffDefOf.NeuralSupercharge);

    private static bool IsCharged(Building supercharger) =>
        supercharger.GetComp<CompNeuralSupercharger>().Charged;

    [Test]
    public static IEnumerator NonOwnerOrderedToUseAnOwnedSuperchargerGivesUp()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(
            autoUseMode: CompNeuralSupercharger.AutoUseMode.NoAutoUse
        );
        var owner = scene.MakeColonist();
        var other = scene.MakeColonist(supercharger.InteractionCell);
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        OrderSupercharge(other, supercharger);
        yield return SuperchargerScene.TickUntil(
            () => !IsGoingToUse(other, supercharger),
            FailTicks
        );

        Expect.IsFalse(IsGoingToUse(other, supercharger), $"{other} is still doing the job");
        Expect.IsFalse(IsSupercharged(other), $"{other} got supercharged");
        Expect.IsTrue(IsCharged(supercharger), "The supercharger was used");
    }

    [Test]
    public static IEnumerator PawnOnItsWayGivesUpWhenTheSuperchargerIsAssignedToSomeoneElse()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(
            autoUseMode: CompNeuralSupercharger.AutoUseMode.NoAutoUse
        );
        var owner = scene.MakeColonist();
        var other = scene.MakeColonist(new IntVec3(45, 0, 45));

        OrderSupercharge(other, supercharger);
        yield return SuperchargerScene.TickUntil(
            () => !IsGoingToUse(other, supercharger),
            FailTicks
        );
        Expect.IsTrue(
            IsGoingToUse(other, supercharger),
            $"{other} stopped before the reassignment"
        );

        supercharger.AssignedOwnership().TryAssignPawn(owner);
        yield return SuperchargerScene.TickUntil(
            () => !IsGoingToUse(other, supercharger),
            FailTicks
        );

        Expect.IsFalse(IsGoingToUse(other, supercharger), $"{other} is still doing the job");
        Expect.IsFalse(IsSupercharged(other), $"{other} got supercharged");
        Expect.IsTrue(IsCharged(supercharger), "The supercharger was used");
    }

    [Test]
    public static IEnumerator OwnerOrderedToUseTheirSuperchargerGetsSupercharged()
    {
        using var scene = new SuperchargerScene(Find.CurrentMap);
        var supercharger = scene.MakeSupercharger(
            autoUseMode: CompNeuralSupercharger.AutoUseMode.NoAutoUse
        );
        var owner = scene.MakeColonist(supercharger.InteractionCell);
        supercharger.AssignedOwnership().TryAssignPawn(owner);

        OrderSupercharge(owner, supercharger);
        yield return SuperchargerScene.TickUntil(() => IsSupercharged(owner), 1000);

        Expect.IsTrue(IsSupercharged(owner), $"{owner} didn't get supercharged");
        Expect.IsFalse(IsCharged(supercharger), "The supercharger wasn't used");
    }
}

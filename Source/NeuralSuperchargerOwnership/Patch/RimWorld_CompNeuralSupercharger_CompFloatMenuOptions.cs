using HarmonyLib;
using NeuralSuperchargerOwnership.Comps;
using Verse.AI;

namespace NeuralSuperchargerOwnership.Patch;

[HarmonyPatch(typeof(CompNeuralSupercharger), nameof(CompNeuralSupercharger.CompFloatMenuOptions))]
internal static class RimWorld_CompNeuralSupercharger_CompFloatMenuOptions
{
    private static bool Prefix(CompNeuralSupercharger __instance, Pawn selPawn, ref IEnumerable<FloatMenuOption> __result, CompProperties ___props)
    {
        var compAssignableToPawn = __instance.parent.TryGetComp<CompAssignableToPawn_NeuralSupercharger>();
        List<Pawn> assignedPawnsForReading = compAssignableToPawn.AssignedPawnsForReading;
        if (assignedPawnsForReading.Empty() || assignedPawnsForReading.Contains(selPawn))
        {
            return true;
        }

        __result = MenuOptions();
        return false;

        IEnumerable<FloatMenuOption> MenuOptions()
        {
            yield return new FloatMenuOption(
                ((CompProperties_NeuralSupercharger)___props).jobString + " (" + "NeuralSuperchargerOwnership.OwnedBySomeoneElse".Translate() + ")", null);
            yield break;
        }
    }
}

[HarmonyPatch(typeof(JobDriver_GetNeuralSupercharge), "MakeNewToils")]
internal static class Rimworld_JobDriver_GetNeuralSupercharge_MakeNewToils
{
    private static void Prefix(JobDriver_GetNeuralSupercharge __instance, Job ___job, Pawn ___pawn)
    {
        __instance.FailOn(() =>
        {
            var charger = ___job.GetTarget(TargetIndex.A).Thing;

            var compAssignableToPawn = charger.TryGetComp<CompAssignableToPawn_NeuralSupercharger>();
            List<Pawn> assignedPawnsForReading = compAssignableToPawn.AssignedPawnsForReading;
            return !assignedPawnsForReading.Empty() && !assignedPawnsForReading.Contains(___pawn);
        });
    }
}

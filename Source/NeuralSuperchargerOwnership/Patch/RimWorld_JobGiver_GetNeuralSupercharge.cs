using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Patch;

// Based on code from
// <https://github.com/PeteTimesSix/TranspilerExamples/blob/main/Source/HarmonyPatchExamples/CompilerGeneratedClasses.cs>

[HarmonyPatch]
internal static class RimWorld_JobGiver_GetNeuralSupercharge
{
    //Check if we can find the methods to patch. Otherwise, Harmony throws an error if given no methods in HarmonyTargetMethods.
    [HarmonyPrepare]
    private static bool ShouldPatch()
    {
        // We can reuse the same method as HarmonyTargetMethods will use afterward.
        var methods = CalculateMethods();

        //check that we have one and only one match. If we get more, the match is giving false positives.
        if (methods.Count() == 1)
        {
            return true;
        }
        else
        {
            NeuralSuperchargerOwnershipMod.Error(
                "Could not patch JobGiver_GetNeuralSupercharge.ClosestSupercharger, could not locate method to patch."
            );
            return false;
        }
    }

    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> CalculateMethods() =>
        typeof(JobGiver_GetNeuralSupercharge)
            .GetNestedTypes(AccessTools.all)
            .SelectMany(AccessTools.GetDeclaredMethods)
            .Where(m =>
#if v1_5
                m.Name.IndexOf("ClosestSupercharger", StringComparison.Ordinal) >= 0
                && m.Name.IndexOf("Validator", StringComparison.Ordinal) >= 0
#else
                m.Name.Contains("ClosestSupercharger", StringComparison.Ordinal)
                && m.Name.Contains("Validator", StringComparison.Ordinal)
#endif
            );

    private static void Postfix(Thing x, ref bool __result, Pawn ___pawn)
    {
        if (__result)
        {
            var compAssignableToPawn = x.TryGetComp<CompAssignableToPawn_NeuralSupercharger>();
            var assignedPawnsForReading = compAssignableToPawn.AssignedPawnsForReading;
            __result = assignedPawnsForReading.Empty() || assignedPawnsForReading.Contains(___pawn);
        }
    }
}

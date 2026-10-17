namespace NeuralSuperchargerOwnership.Patch;

// Runs where vanilla generates its implied defs, before def references are resolved.
[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
internal static class RimWorld_DefGenerator_GenerateImpliedDefs_PreResolve
{
    private static void Postfix() =>
        OwnershipCompInjector.AddOwnershipComp(DefDatabase<ThingDef>.AllDefsListForReading);
}

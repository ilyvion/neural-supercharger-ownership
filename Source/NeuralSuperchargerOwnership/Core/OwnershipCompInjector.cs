using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership;

// Gives every humanlike race, including modded races such as Humanoid Alien Races ones, the
// comp that tracks which neural supercharger a pawn owns.
internal static class OwnershipCompInjector
{
    internal static void AddOwnershipComp(IEnumerable<ThingDef> defs)
    {
        foreach (var def in defs)
        {
            if (def.race?.Humanlike != true)
            {
                continue;
            }

            def.comps ??= [];
            if (!def.comps.Any(c => c.compClass == typeof(CompNeuralSuperchargerOwnership)))
            {
                def.comps.Add(
                    new CompProperties { compClass = typeof(CompNeuralSuperchargerOwnership) }
                );
            }
        }
    }
}

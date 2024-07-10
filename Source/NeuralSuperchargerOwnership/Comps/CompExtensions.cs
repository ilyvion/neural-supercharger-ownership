namespace NeuralSuperchargerOwnership.Comps;

internal static class CompExtensions
{
    public static CompNeuralSuperchargerOwnership NeuralSuperchargerOwnership(this Pawn pawn)
    {
        return pawn.GetComp<CompNeuralSuperchargerOwnership>();
    }

    public static CompAssignableToPawn_NeuralSupercharger AssignedOwnership(this Building building)
    {
        return building.GetComp<CompAssignableToPawn_NeuralSupercharger>();
    }
}

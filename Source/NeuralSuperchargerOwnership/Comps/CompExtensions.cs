namespace NeuralSuperchargerOwnership.Comps;

internal static class CompExtensions
{
    public static CompNeuralSuperchargerOwnership NeuralSuperchargerOwnership(this Pawn pawn) =>
        pawn.GetComp<CompNeuralSuperchargerOwnership>();

    public static CompAssignableToPawn_NeuralSupercharger AssignedOwnership(
        this Building building
    ) => building.GetComp<CompAssignableToPawn_NeuralSupercharger>();
}

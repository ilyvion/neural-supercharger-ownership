namespace NeuralSuperchargerOwnership.Comps;

public class CompNeuralSuperchargerOwnership : ThingComp
{
    internal Building? ownedNeuralSupercharger;

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_References.Look(ref ownedNeuralSupercharger, "ownedNeuralSupercharger");
    }

    internal bool ClaimNeuralSupercharger(Building newNeuralSupercharger)
    {
        var compAssignable = newNeuralSupercharger.AssignedOwnership();
        if (compAssignable.IsOwner((Pawn)parent))
        {
            return false;
        }
        UnclaimNeuralSupercharger();
        if (!compAssignable.AssignedPawnsForReading.Empty())
        {
            compAssignable.AssignedPawnsForReading[0].NeuralSuperchargerOwnership().UnclaimNeuralSupercharger();
        }
        compAssignable.ForceAddPawn((Pawn)parent);
        ownedNeuralSupercharger = newNeuralSupercharger;
        return true;
    }

    internal bool UnclaimNeuralSupercharger()
    {
        if (ownedNeuralSupercharger == null)
        {
            return false;
        }
        ownedNeuralSupercharger.AssignedOwnership().ForceRemovePawn((Pawn)parent);
        ownedNeuralSupercharger = null;
        return true;
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        UnclaimNeuralSupercharger();
    }
}

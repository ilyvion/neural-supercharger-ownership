namespace NeuralSuperchargerOwnership.Comps;

public class CompAssignableToPawn_NeuralSupercharger : CompAssignableToPawn
{
    public static Dictionary<Pawn, Building> AssignedNeuralSuperchargers { get; private set; } = [];
    public static new Dictionary<Building, Pawn> AssignedPawns { get; private set; } = [];

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Building building = (Building)parent;

            if (assignedPawns.Count == 1)
            {
                AssignedNeuralSuperchargers[assignedPawns[0]] = building;
                AssignedPawns[building] = assignedPawns[0];
            }
        }
    }

    protected override string GetAssignmentGizmoDesc()
    {
        return "NeuralSuperchargerOwnership.CommandBedSetOwnerDesc".Translate(FactionDefOf.PlayerColony.pawnSingular);
    }

    public override bool AssignedAnything(Pawn pawn)
    {
        return AssignedNeuralSuperchargers.ContainsKey(pawn);
    }

    public override void ForceAddPawn(Pawn pawn)
    {
        TryAssignPawn(pawn);
    }

    public override void TryAssignPawn(Pawn pawn)
    {
        Building building = (Building)parent;

        // Remove the current owner of this building
        if (AssignedPawns.TryGetValue(building, out var currentOwner))
        {
            AssignedNeuralSuperchargers.Remove(currentOwner);
        }
        if (!assignedPawns.Empty())
        {
            assignedPawns.Clear();
        }

        // Remove the pawn from other buildings they may already own
        if (AssignedNeuralSuperchargers.TryGetValue(pawn, out var currentBuilding))
        {
            currentBuilding.GetComp<CompAssignableToPawn_NeuralSupercharger>().assignedPawns.Clear();
        }

        AssignedNeuralSuperchargers[pawn] = building;
        AssignedPawns[building] = pawn;
        assignedPawns.Add(pawn);

        uninstalledAssignedPawns.Remove(pawn);
    }

    public override void ForceRemovePawn(Pawn pawn)
    {
        TryUnassignPawn(pawn);
    }

    public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
    {
        if (AssignedNeuralSuperchargers.TryGetValue(pawn, out var currentBuilding))
        {
            AssignedPawns.Remove(currentBuilding);
        }
        AssignedNeuralSuperchargers.Remove(pawn);
        assignedPawns.Remove(pawn);
        if (uninstall && !uninstalledAssignedPawns.Contains(pawn))
        {
            uninstalledAssignedPawns.Add(pawn);
        }
    }

    protected override bool CanSetUninstallAssignedPawn(Pawn pawn)
    {
        return true;
    }

    public override string CompInspectStringExtra()
    {
        if (!PlayerCanSeeAssignments)
        {
            return "";
        }

        Building building = (Building)parent;
        if (AssignedPawns.TryGetValue(building, out var currentOwner))
        {
            if (CanDrawOverlayForPawn(currentOwner))
            {
                return "Owner".Translate() + ": " + currentOwner.Label;
            }
            else
            {
                return "";
            }
        }
        else
        {
            return "Owner".Translate() + ": " + "Nobody".Translate();
        }
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        RemoveEntries();
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);
        RemoveEntries();
    }

    private void RemoveEntries()
    {
        Building building = (Building)parent;
        AssignedPawns.Remove(building);
        var pawn = AssignedNeuralSuperchargers
            .Where(kv => kv.Value == building)
            .Select(kv => kv.Key)
            .SingleOrDefault();
        if (pawn != null)
        {
            AssignedNeuralSuperchargers.Remove(pawn);
        }
    }
}

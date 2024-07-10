namespace NeuralSuperchargerOwnership.Comps;

[HotSwappable]
public class CompAssignableToPawn_NeuralSupercharger : CompAssignableToPawn
{
    internal bool IsOwner(Pawn pawn)
    {
        return AssignedPawnsForReading.IndexOf(pawn) >= 0;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            Building building = (Building)parent;

            if (assignedPawns.Count == 1)
            {
                var assignedPawn = assignedPawns[0];
                if (assignedPawn.NeuralSuperchargerOwnership().ownedNeuralSupercharger != building)
                {
                    NeuralSuperchargerOwnershipMod.Warning($"Neural supercharger {building} was assigned to pawn {assignedPawn}, but pawn didn't own it. This should only be the case when upgrading from version 0.1.0 to 0.2.0! Assigning ownership to pawn now.");
                    assignedPawn.NeuralSuperchargerOwnership().ownedNeuralSupercharger = building;
                }
            }
        }
    }

    protected override string GetAssignmentGizmoDesc()
    {
        return "NeuralSuperchargerOwnership.CommandBedSetOwnerDesc".Translate(FactionDefOf.PlayerColony.pawnSingular);
    }

    public override bool AssignedAnything(Pawn pawn)
    {
        return (pawn ?? throw new ArgumentNullException(nameof(pawn))).NeuralSuperchargerOwnership().ownedNeuralSupercharger != null;
    }

    public override void TryAssignPawn(Pawn pawn)
    {
        Building building = (Building)parent;

        (pawn ?? throw new ArgumentNullException(nameof(pawn))).NeuralSuperchargerOwnership().ClaimNeuralSupercharger(building);
        uninstalledAssignedPawns.Remove(pawn);
    }

    public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
    {
        (pawn ?? throw new ArgumentNullException(nameof(pawn))).NeuralSuperchargerOwnership().UnclaimNeuralSupercharger();
        if (uninstall && !uninstalledAssignedPawns.Contains(pawn))
        {
            uninstalledAssignedPawns.Add(pawn);
        }
    }

    protected override bool CanSetUninstallAssignedPawn(Pawn pawn)
    {
        if (pawn != null && !AssignedAnything(pawn) && (bool)CanAssignTo(pawn))
        {
            if (!pawn.IsPrisonerOfColony)
            {
                return pawn.IsColonist;
            }
            return true;
        }
        return false;
    }

    public override string CompInspectStringExtra()
    {
        if (!PlayerCanSeeAssignments)
        {
            return string.Empty;
        }

        if (assignedPawns.Count == 1)
        {
            var assignedPawn = assignedPawns[0];
            if (CanDrawOverlayForPawn(assignedPawn))
            {
                return "Owner".Translate() + ": " + assignedPawn.Label;
            }
            else
            {
                return string.Empty;
            }
        }
        else
        {
            return "Owner".Translate() + ": " + "Nobody".Translate();
        }
    }

    public override void PostDeSpawn(Map map)
    {
        // Intentionally not calling this; handling removal of assignment in PostDestroy
        // base.PostDeSpawn(map);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);

        bool destroyed = mode == DestroyMode.KillFinalize;

        if (mode != 0)
        {
            if (assignedPawns.Count == 1)
            {
                var assignedPawn = assignedPawns[0];
                assignedPawn.NeuralSuperchargerOwnership().UnclaimNeuralSupercharger();

                string key = "MessageBedLostAssignment";
                if (destroyed)
                {
                    key = "MessageBedDestroyed";
                }
                Messages.Message(key.Translate(parent.def, assignedPawn), new LookTargets(parent, assignedPawn), MessageTypeDefOf.CautionInput, historical: false);
            }
        }
        else if (InstallBlueprintUtility.ExistingBlueprintFor(parent) == null)
        {
            if (assignedPawns.Count == 1)
            {
                var assignedPawn = assignedPawns[0];
                Messages.Message("MessageBedLostAssignment".Translate(parent.def, assignedPawn), new LookTargets(parent, assignedPawn), MessageTypeDefOf.CautionInput, historical: false);
            }
        }
    }

    public override void DrawGUIOverlay()
    {
        if (Find.CameraDriver.CurrentZoom != 0 || !PlayerCanSeeAssignments)
        {
            return;
        }

        if (assignedPawns.Count == 1)
        {
            Building building = (Building)parent;
            bool charged = building.GetComp<CompNeuralSupercharger>().Charged;
            Pawn pawn = assignedPawns[0];
            if (CanDrawOverlayForPawn(pawn) && (!pawn.RaceProps.Animal || Prefs.AnimalNameMode.ShouldDisplayAnimalName(pawn)))
            {
                if (charged && parent.Rotation.IsHorizontal)
                {
                    GenMapUI.DrawThingLabel(parent, pawn.LabelShort, GenMapUI.DefaultThingLabelColor);
                }
                else
                {
                    Vector2 screenPos = GenMapUI.LabelDrawPosFor(parent, -0.4f);
                    if (parent.Rotation.IsHorizontal)
                    {
                        screenPos.y -= 15f;
                    }
                    else
                    {
                        screenPos.y += 30f;
                    }
                    GenMapUI.DrawThingLabel(screenPos, pawn.LabelShort, GenMapUI.DefaultThingLabelColor);
                }
            }
        }
        else
        {
            base.DrawGUIOverlay();
        }
    }
}

namespace NeuralSuperchargerOwnership.Comps;

[HotSwappable]
internal sealed class CompAssignableToPawn_NeuralSupercharger : CompAssignableToPawn
{
    internal bool IsOwner(Pawn pawn) => assignedPawns.IndexOf(pawn) >= 0;

    internal bool MayBeUsedBy(Pawn pawn) => MayUse(assignedPawns, pawn);

    // A supercharger without an owner may be used by anyone.
    internal static bool MayUse<T>(IReadOnlyList<T> owners, T pawn) =>
        owners.Count == 0 || owners.Contains(pawn);

    public override void PostExposeData()
    {
        base.PostExposeData();

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            var building = (Building)parent;

            if (assignedPawns.Count == 1)
            {
                var assignedPawn = assignedPawns[0];
                if (assignedPawn.NeuralSuperchargerOwnership().ownedNeuralSupercharger != building)
                {
                    NeuralSuperchargerOwnershipMod.Warning(
                        $"Neural supercharger {building} was assigned to pawn {assignedPawn}, but pawn didn't own it. This should only be the case when upgrading from version 0.1.0 to 0.2.0! Assigning ownership to pawn now."
                    );
                    assignedPawn.NeuralSuperchargerOwnership().ownedNeuralSupercharger = building;
                }
            }
        }
    }

    protected override string GetAssignmentGizmoDesc() =>
        "NeuralSuperchargerOwnership.CommandBedSetOwnerDesc".Translate(
            FactionDefOf.PlayerColony.pawnSingular
        );

    public override bool AssignedAnything(Pawn pawn) =>
        (pawn ?? throw new ArgumentNullException(nameof(pawn)))
            .NeuralSuperchargerOwnership()
            .ownedNeuralSupercharger != null;

    public override void TryAssignPawn(Pawn pawn)
    {
        var building = (Building)parent;

        _ = (pawn ?? throw new ArgumentNullException(nameof(pawn)))
            .NeuralSuperchargerOwnership()
            .ClaimNeuralSupercharger(building);
        _ = uninstalledAssignedPawns.Remove(pawn);
    }

    public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
    {
        if (!IsOwner(pawn ?? throw new ArgumentNullException(nameof(pawn))))
        {
            return;
        }

        _ = pawn.NeuralSuperchargerOwnership().UnclaimNeuralSupercharger();
        if (uninstall && !uninstalledAssignedPawns.Contains(pawn))
        {
            uninstalledAssignedPawns.Add(pawn);
        }
    }

    protected override bool CanSetUninstallAssignedPawn(Pawn pawn) =>
        pawn != null
        && !AssignedAnything(pawn)
        && (bool)CanAssignTo(pawn)
        && (pawn.IsPrisonerOfColony || pawn.IsColonist);

    public override string CompInspectStringExtra()
    {
        if (!PlayerCanSeeAssignments)
        {
            return string.Empty;
        }

        if (assignedPawns.Count == 1)
        {
            var assignedPawn = assignedPawns[0];
            return CanDrawOverlayForPawn(assignedPawn)
                ? "Owner".Translate() + ": " + assignedPawn.Label
                : string.Empty;
        }
        else
        {
            return "Owner".Translate() + ": " + "Nobody".Translate();
        }
    }

#if v1_5
    public override void PostDeSpawn(Map map)
#else
    public override void PostDeSpawn(Map map, DestroyMode mode)
#endif
    {
        // Intentionally not calling this; handling removal of assignment in PostDestroy
        // base.PostDeSpawn(map);
    }

    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        base.PostDestroy(mode, previousMap);

        if (assignedPawns.Count != 1)
        {
            return;
        }

        // Every destroy mode removes the supercharger for good; minifying only despawns it.
        var assignedPawn = assignedPawns[0];
        _ = assignedPawn.NeuralSuperchargerOwnership().UnclaimNeuralSupercharger();

        string key;
        if (mode == DestroyMode.KillFinalize)
        {
            key = "MessageBedDestroyed";
        }
        else if (
            mode != DestroyMode.Vanish
            || InstallBlueprintUtility.ExistingBlueprintFor(parent) == null
        )
        {
            key = "MessageBedLostAssignment";
        }
        else
        {
            return;
        }
        Messages.Message(
            key.Translate(parent.def, assignedPawn),
            new LookTargets(parent, assignedPawn),
            MessageTypeDefOf.CautionInput,
            historical: false
        );
    }

    public override void DrawGUIOverlay()
    {
        if (Find.CameraDriver.CurrentZoom != 0 || !PlayerCanSeeAssignments)
        {
            return;
        }

        if (assignedPawns.Count == 1)
        {
            var building = (Building)parent;
            var charged = building.GetComp<CompNeuralSupercharger>().Charged;
            var pawn = assignedPawns[0];
            if (
                CanDrawOverlayForPawn(pawn)
                && (!pawn.RaceProps.Animal || Prefs.AnimalNameMode.ShouldDisplayAnimalName(pawn))
            )
            {
                if (charged && parent.Rotation.IsHorizontal)
                {
                    GenMapUI.DrawThingLabel(
                        parent,
                        pawn.LabelShort,
                        GenMapUI.DefaultThingLabelColor
                    );
                }
                else
                {
                    var screenPos = GenMapUI.LabelDrawPosFor(parent, -0.4f);
                    if (parent.Rotation.IsHorizontal)
                    {
                        screenPos.y -= 15f;
                    }
                    else
                    {
                        screenPos.y += 30f;
                    }
                    GenMapUI.DrawThingLabel(
                        screenPos,
                        pawn.LabelShort,
                        GenMapUI.DefaultThingLabelColor
                    );
                }
            }
        }
        else
        {
            base.DrawGUIOverlay();
        }
    }
}

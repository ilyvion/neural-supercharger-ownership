using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// Player-owned neural superchargers and colonists for the ownership tests. Disposing destroys
// everything the scene made, so the next test on the same map sees none of it.
internal sealed class SuperchargerScene : IDisposable
{
    private readonly List<Building> superchargers = [];
    private readonly List<Pawn> pawns = [];
    private readonly TimeSpeed timeSpeed;

    internal Map Map { get; }

    internal SuperchargerScene(Map map)
    {
        Map = map;
        timeSpeed = Find.TickManager.CurTimeSpeed;
        Expect.IsEmpty(
            map.listerThings.ThingsOfDef(ThingDefOf.NeuralSupercharger),
            "An earlier test leaked a neural supercharger onto the map"
        );
        ExpectNoColonistOwnsASupercharger("An earlier test left a colonist owning a supercharger");
    }

    // Charge is set directly, so the supercharger needs no power.
    internal Building MakeSupercharger(
        bool spawn = true,
        bool charged = true,
        CompNeuralSupercharger.AutoUseMode autoUseMode =
            CompNeuralSupercharger.AutoUseMode.AutoUseForEveryone
    )
    {
        var building = (Building)ThingMaker.MakeThing(ThingDefOf.NeuralSupercharger);
        building.SetFaction(Faction.OfPlayer);
        Expect.IsNotNull(
            building.AssignedOwnership(),
            "The neural supercharger has no CompAssignableToPawn_NeuralSupercharger"
        );

        var compSupercharger = building.GetComp<CompNeuralSupercharger>();
        compSupercharger.autoUseMode = autoUseMode;
        if (charged)
        {
            compSupercharger.ticksUntilCharged = 0;
        }

        if (spawn)
        {
            // Superchargers are 1x3 with their interaction cell in front, so lining them up 3 cells
            // apart, away from the starting colonists at the map's center, keeps them clear of
            // each other and of everything else.
            var cell = new IntVec3(15 + (3 * superchargers.Count), 0, 15);
            _ = GenSpawn.Spawn(building, cell, Map, Rot4.North);
        }
        superchargers.Add(building);
        return building;
    }

    // Without a cell, the colonist is spawned somewhere near the map's center.
    internal Pawn MakeColonist(IntVec3? cell = null) =>
        MakePawn(PawnKindDefOf.Colonist, Faction.OfPlayer, cell);

    // The test world has no other humanlike factions, so strangers belong to no faction.
    internal Pawn MakeStranger() => MakePawn(PawnKindDefOf.Villager, null, null);

    internal Pawn MakePrisoner()
    {
        var pawn = MakeStranger();
        pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
        return pawn;
    }

    private Pawn MakePawn(PawnKindDef kind, Faction? faction, IntVec3? cell)
    {
        var pawn = PawnGenerator.GeneratePawn(kind, faction);
        Expect.IsNotNull(
            pawn.NeuralSuperchargerOwnership(),
            "The pawn has no CompNeuralSuperchargerOwnership"
        );
        _ = GenSpawn.Spawn(
            pawn,
            cell ?? CellFinder.RandomClosewalkCellNear(new IntVec3(20, 0, 20), Map, 8),
            Map
        );
        pawns.Add(pawn);
        return pawn;
    }

    // Whatever the pawn carries is destroyed first, so dying drops nothing onto the map. The corpse
    // is destroyed by Dispose.
    internal static void Kill(Pawn pawn)
    {
        pawn.equipment?.DestroyAllEquipment();
        pawn.inventory?.DestroyAll();
        pawn.carryTracker?.DestroyCarriedThing();
        pawn.Kill(null);
    }

    // Pauses the game and ticks it by hand until the condition holds or maxTicks have passed,
    // letting a frame pass every 60 ticks.
    internal static IEnumerator TickUntil(Func<bool> condition, int maxTicks)
    {
        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
        for (var ticks = 0; ticks < maxTicks && !condition(); ticks++)
        {
            Find.TickManager.DoSingleTick();
            if (ticks % 60 == 59)
            {
                yield return null;
            }
        }
    }

    // Ownership is recorded both on the pawn and on the supercharger, and both must agree.
    internal void ExpectConsistent(Pawn pawn, Building? building)
    {
        Expect.IsTrue(
            pawn.NeuralSuperchargerOwnership().ownedNeuralSupercharger == building,
            $"{pawn} doesn't own {building?.ToString() ?? "nothing"}"
        );
        foreach (var supercharger in superchargers)
        {
            var owners = supercharger.AssignedOwnership().AssignedPawnsForReading;
            if (supercharger == building)
            {
                Expect.AreEqual(1, owners.Count, $"{supercharger} doesn't have exactly one owner");
                Expect.IsTrue(owners.Contains(pawn), $"{supercharger} doesn't list {pawn}");
            }
            else
            {
                Expect.IsFalse(owners.Contains(pawn), $"{supercharger} lists {pawn}");
            }
        }
    }

    private void ExpectNoColonistOwnsASupercharger(string message) =>
        Expect.None(
            Map.mapPawns.FreeColonists,
            pawn => pawn.NeuralSuperchargerOwnership()?.ownedNeuralSupercharger != null,
            message
        );

    // Pawns go first, so they give up their superchargers before those are destroyed.
    public void Dispose()
    {
        foreach (var pawn in pawns)
        {
            if (!pawn.Destroyed)
            {
                pawn.Destroy();
            }
            if (pawn.Corpse is { Destroyed: false } corpse)
            {
                corpse.Destroy();
            }
        }
        foreach (var supercharger in superchargers)
        {
            if (!supercharger.Destroyed)
            {
                supercharger.Destroy();
            }
        }
        ExpectNoColonistOwnsASupercharger("A colonist still owns a destroyed supercharger");
        Find.TickManager.CurTimeSpeed = timeSpeed;
    }
}

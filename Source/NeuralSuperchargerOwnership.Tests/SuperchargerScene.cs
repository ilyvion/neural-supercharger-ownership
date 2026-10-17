using DevTools.Testing;
using NeuralSuperchargerOwnership.Comps;

namespace NeuralSuperchargerOwnership.Tests;

// Player-owned neural superchargers and colonists for the ownership tests. Disposing destroys
// everything the scene made, so the next test on the same map sees none of it.
internal sealed class SuperchargerScene : IDisposable
{
    private readonly List<Building> superchargers = [];
    private readonly List<Pawn> colonists = [];

    internal Map Map { get; }

    internal SuperchargerScene(Map map)
    {
        Map = map;
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

    internal Pawn MakeColonist()
    {
        var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
        Expect.IsNotNull(
            pawn.NeuralSuperchargerOwnership(),
            "The colonist has no CompNeuralSuperchargerOwnership"
        );
        var cell = CellFinder.RandomClosewalkCellNear(new IntVec3(20, 0, 20), Map, 8);
        _ = GenSpawn.Spawn(pawn, cell, Map);
        colonists.Add(pawn);
        return pawn;
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

    // Colonists go first, so they give up their superchargers before those are destroyed.
    public void Dispose()
    {
        foreach (var pawn in colonists)
        {
            if (!pawn.Destroyed)
            {
                pawn.Destroy();
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
    }
}

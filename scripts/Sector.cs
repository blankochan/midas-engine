using Godot;
using System;
using System.Linq;

public partial class Sector : Node
{
    public static PackedScene sectorPrototype;

    public byte id;
    public Tile[] tiles;

    public static Sector NewSector(byte id)
    {
        // Create new copy of sector
        Sector sector = (Sector)sectorPrototype.Instantiate();

        sector.Name = "Sector " + id.ToString();

        // Creating all the tiles within
        sector.tiles = new Tile[PlanetInfo.tilesPerSector];

        for (uint i = 0; i < PlanetInfo.tilesPerSector; i++)
        {
            Tile tile = Tile.NewTile(id, i);
            sector.tiles[i] = tile;
            sector.AddChild(Tile.NewTile(id, i));
        }
        return sector;
    }

}

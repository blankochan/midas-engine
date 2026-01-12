using Godot;
using System;

// The information of a tile, separate from the GameObject, so we can manipulate it without spawning Objects
public struct TileInfo
{
    public static byte[,] sectorLinks =
{   // I   J   K
        
        // Upper Ring
        {05, 01, 04 }, // 00
        {06, 02, 00 }, // 01
        {07, 03, 01 }, // 02
        {08, 04, 02 }, // 03
        {09, 00, 03 }, // 04

        // Belt / Equator
        {00, 14, 10 }, // 05
        {01, 10, 11 }, // 06
        {02, 11, 12 }, // 07
        {03, 12, 13 }, // 08
        {04, 13, 14 }, // 09
        {15, 06, 05 }, // 10
        {16, 07, 06 }, // 11
        {17, 08, 07 }, // 12
        {18, 09, 08 }, // 13
        {19, 10, 09 }, // 14

        // Lower Ring
        {10, 19, 16 }, // 15
        {11, 15, 17 }, // 16
        {12, 16, 18 }, // 17
        {13, 17, 19 }, // 18
        {14, 18, 15 }  // 19
    };

    public byte sector;
    public uint asperaCoords;

    // Constructor
    public TileInfo(byte sector, uint asperaCoords)
    {
        this.sector = sector;
        this.asperaCoords = asperaCoords;
    }

    // Neighbor searching
    public TileInfo FindNeighbor(uint direction)
    {
        TileInfo result = new TileInfo(this.sector, this.asperaCoords);

        for (int i = 0; i < PlanetInfo.depth; i++)
        {
            uint v = (uint)(result.asperaCoords & (0b11 << i * 2));

            result.asperaCoords = result.asperaCoords ^ direction;

            if (v == direction || v == 0) break;

            if (i == PlanetInfo.depth)
            {
                // Sector changing
                result.sector = sectorLinks[this.sector, direction];
                // Maybe rotation if that is necessary
            }

            direction = direction << 2;
        }

        return result;
    }

    // Gets the Xth move, where the leftmost move is x = 0
    public byte GetMove(byte move)
    {
        uint mask = (uint)(3 << 2 * move);
        return (byte)((asperaCoords & mask) / (byte)Math.Pow(4, move));
    }
}


public partial class Tile : Node
{
    public static PackedScene tilePrototype;

    public MeshInstance3D visuals;
    public StaticBody3D physics;
    public CollisionShape3D collider;

    // The relationships between sectors, hard-coded.


    // Variables
    public TileInfo info;
    /*  Deeper explanation of Aspera Coordinates
     *  
     *  Every movement is represented by two bits, with the leftmost being the most significant.
     *  o = 00
     *  i = 01
     *  j = 10
     *  k = 11  
     */

    // Constructors
    public static Tile NewTile(byte sector, uint asperaCoords)
    {
        Tile tile = (Tile)tilePrototype.Instantiate();

        // Name
        string name = "Tile "; name += sector.ToString(); name += "-"; name += asperaCoords.ToString();
        tile.Name = name;

        // Onready
        tile.visuals = (MeshInstance3D)tile.GetChild(0);
        tile.physics = (StaticBody3D)tile.GetChild(1);
        tile.collider = (CollisionShape3D)tile.physics.GetChild(0);

        // Memory
        tile.info.sector = sector;
        tile.info.asperaCoords = asperaCoords;

        return tile;
    }

    // Methods

    // Sets the visuals to a triangle of 3 points
    public void InitializeVisuals(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        Vector3[] vertices = { pointA, pointB, pointC };

        var arrMesh = new ArrayMesh();

        Godot.Collections.Array arrays = [];

        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;

        arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        visuals.Mesh = arrMesh;
        
        // Physics

        ConvexPolygonShape3D shape = visuals.Mesh.CreateConvexShape();
        collider.Shape = shape;
    }

    public void select()
    {
        StandardMaterial3D newMaterial = new StandardMaterial3D();
        newMaterial.AlbedoColor = new Color(1f, 0f, 0f);
        visuals.MaterialOverride = newMaterial;
    }

}

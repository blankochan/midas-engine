using Godot;
using System;

public struct PlanetInfo
{
    public static byte depth;

    public static uint tilesPerSector;
    public static uint pointsPerEdge;
}

public partial class PlanetManager : Node
{

    [Export]
    public NodePath planetPath;
    public Node planet;

    public Node theGrid;
    public Sector[] sectors = new Sector[20];

    [Export]
    public PackedScene sectorPrototype;
    [Export]
    public PackedScene tileProtoype;

    [Export]
    public PackedScene pointTest;

    [Export(PropertyHint.Range, "0,8,")]
    public byte depth;

    // Edges (Going in the direction of I)
    static byte[,] edgesDefinition =
    {
        // Upper ring
        {00, 01},   // 00
        {01, 02},   // 01
        {00, 02},   // 02
        {02, 03},   // 03
        {00, 03},   // 04
        {03, 04},   // 05
        {00, 04},   // 06
        {04, 05},   // 07
        {00, 05},   // 08
        {05, 01},   // 09
        // Belt (The zigzag)
        {01, 06},   // 10
        {02, 06},   // 11
        {02, 07},   // 12
        {03, 07},   // 13
        {03, 08},   // 14
        {04, 08},   // 15
        {04, 09},   // 16
        {05, 09},   // 17
        {05, 10},   // 18
        {01, 10},   // 19
        // Lower ring
        {11, 06},   // 20
        {06, 07},   // 21
        {11, 07},   // 22
        {07, 08},   // 23
        {11, 08},   // 24
        {08, 09},   // 25
        {11, 09},   // 26
        {09, 10},   // 27
        {11, 10},   // 28
        {10, 06}    // 29
    };

    static byte[,] facesDefinition =
    {
            //  {Edge1, Edge2, Edge3, UpsideDown (If Edges 1 and 2 go away from I), Humor (If J corner is first on the Third Edge)}
        // Upper ring
        {00, 02, 01, 00, 00},   // 00
        {02, 04, 03, 00, 00},   // 01
        {04, 06, 05, 00, 00},   // 02
        {06, 08, 07, 00, 00},   // 03
        {08, 00, 09, 00, 00},   // 04
        // Ring        , 00
        {10, 11, 01, 01, 01},   // 05
        {12, 13, 03, 01, 01},   // 06
        {14, 15, 05, 01, 01},   // 07
        {16, 17, 07, 01, 01},   // 08
        {18, 19, 09, 01, 01},   // 09
        {11, 12, 21, 00, 00},   // 10
        {13, 14, 23, 00, 00},   // 11
        {15, 16, 25, 00, 00},   // 12
        {17, 18, 27, 00, 00},   // 13
        {19, 10, 29, 00, 00},   // 14
        // Lower right , 00
        {20, 22, 21, 00, 01},   // 15
        {22, 24, 23, 00, 01},   // 16
        {24, 26, 25, 00, 01},   // 17
        {26, 28, 27, 00, 01},   // 18
        {28, 20, 29, 00, 01}    // 19
    }; // This is defined by the J and K edges


    public override void _Ready()
    {
        PlanetInfo.depth = this.depth;
        PlanetInfo.tilesPerSector = (uint)Math.Pow(4, PlanetInfo.depth);
        PlanetInfo.pointsPerEdge = (uint)Math.Pow(2, PlanetInfo.depth) + 1;
        GD.Print("P/E: ", PlanetInfo.pointsPerEdge);

        planet = GetNode(planetPath);
        theGrid = planet.GetChild(0);

        Sector.sectorPrototype = this.sectorPrototype;
        Tile.tilePrototype = this.tileProtoype;


        // Initialize The Grid
        for (byte i = 0; i < 20; i++)
        {
            Sector sector = Sector.NewSector(i);
            sectors[i] = sector;
            theGrid.AddChild(Sector.NewSector(i));
        }

        // Visuals
        Vector3[,,] allPoints = new Vector3[20, PlanetInfo.pointsPerEdge, PlanetInfo.pointsPerEdge]; // [FACE, ROW, COL]

        Vector3[] basePoints = new Vector3[12];
        float quirk0 = (float)(1 / Math.Sqrt(5));
        float quirk1 = (float)((5 - Math.Sqrt(5)) / 10);
        float quirk2 = (float)((-5 - Math.Sqrt(5)) / 10);
        float quirk3 = (float)(Math.Sqrt( (5 - Math.Sqrt(5)) / 10));
        float quirk4 = (float)(Math.Sqrt( (5 + Math.Sqrt(5)) / 10));

        /*
        
        So far, each face looks like this:



        */



        // First 12 are the base points of an icosahedron (In brackets for collapsability
        {
            basePoints[0] = new Vector3(0, 1, 0);
            basePoints[1] = new Vector3(-quirk3, quirk0, -quirk2);
            basePoints[2] = new Vector3(quirk3, quirk0, -quirk2);
            basePoints[3] = new Vector3(quirk4, quirk0, -quirk1);
            basePoints[4] = new Vector3(0, quirk0, -2 * quirk0);
            basePoints[5] = new Vector3(-quirk4, quirk0, -quirk1);

            basePoints[6] = new Vector3(0, -quirk0, 2 * quirk0);
            basePoints[7] = new Vector3(quirk4, -quirk0, quirk1);
            basePoints[8] = new Vector3(quirk3, -quirk0, quirk2);
            basePoints[9] = new Vector3(-quirk3, -quirk0, quirk2);
            basePoints[10] = new Vector3(-quirk4, -quirk0, quirk1);
            basePoints[11] = new Vector3(0, -1, 0);
        }

        for (int i = 0; i < 12; i++)
        {
            /*Node3D node = (Node3D)pointTest.Instantiate();
            AddChild(node);
            node.GlobalPosition = basePoints[i];*/
        }

        Vector3[,] edgeUniquePoints = new Vector3[30, PlanetInfo.pointsPerEdge - 2]; // Is 2 smaller then allPoints because it does not include basePoints

        for (int edge = 0; edge < 30; edge++)
        {
            Vector3 origin = basePoints[edgesDefinition[edge, 0]];
            Vector3 destination = basePoints[edgesDefinition[edge, 1]];
            for (int i = 0; i < PlanetInfo.pointsPerEdge - 2; i++)
            {
                edgeUniquePoints[edge, i] = origin.Lerp(destination, (float)(i + 1) / (float)(PlanetInfo.pointsPerEdge - 1)); // Is 1 smaller because 5 points have 4 gaps between them
            }
        }

        // Getting all the points in this format: [FACE, ROW, COL]
        // The ROW and COL is determined off the J and K edges

        if (depth > 1)
        {

            // [Face, Row, Col]
            Vector3[,,] facePoints = new Vector3[20, PlanetInfo.pointsPerEdge - 3, PlanetInfo.pointsPerEdge - 3]; // Is 3 smaller then allPoints because it does not include basePoints or edgePoints

            for (int face = 0; face < 20; face++)
            {
                bool upsideDown = facesDefinition[face, 3] == 1;
                for (int row = 0; row < PlanetInfo.pointsPerEdge - 3; row++) // Is 3 smaller then allPoints because it does not include basePoints or edgePoints
                {
                    // row is incremented because the grid of the face points starts one index later then the edges
                    // the first edges 
                    Vector3 origin = edgeUniquePoints[facesDefinition[face, 0], row + 1];
                    Vector3 destination = edgeUniquePoints[facesDefinition[face, 1], row + 1]; // + 1 because we skip the first two rows of the face, with not face points.
                    if (upsideDown)
                    {
                        origin = edgeUniquePoints[facesDefinition[face, 0], PlanetInfo.pointsPerEdge - row - 4];       
                        destination = edgeUniquePoints[facesDefinition[face, 1], PlanetInfo.pointsPerEdge - row - 4]; // -1 because we get the last point of the edge, -row because we are on the opposite side
                    }
                    for (int col = 0; col < row + 1; col++)
                    {
                        facePoints[face, row, col] = origin.Lerp(destination, (float)(col + 1) / (float)(row + 2));
                    }
                }
            }

            // All points generation
            // TODO: FIX
            for (byte face = 0; face < 20; face++)
            {
                
                bool upsideDown = facesDefinition[face, 3] == 1;
                bool flipped = facesDefinition[face, 4] == 1;

                // First row, just the I corner.
                if (upsideDown) allPoints[face, 0, 0] = basePoints[edgesDefinition[facesDefinition[face, 0], 1]];
                else allPoints[face, 0, 0] = basePoints[edgesDefinition[facesDefinition[face, 0], 0]];

                // Middle rows
                for (int row = 1; row < PlanetInfo.pointsPerEdge - 1; row++) // -1 to cull out the final row
                {

                    // Left edge
                    if (upsideDown) allPoints[face, row, 0] = edgeUniquePoints[facesDefinition[face, 0], PlanetInfo.pointsPerEdge - row - 2];
                    else allPoints[face, row, 0] = edgeUniquePoints[facesDefinition[face, 0], row - 1];

                    // Face Points, only starts when row = 2
                    for (int col = 1; col < row; col++) // -1 to cull out the right edge
                    {
                        allPoints[face, row, col] = facePoints[face, row - 2, col - 1]; // We cut out the first two rows, and the first column
                    }
                    
                    // Right edge
                    if (upsideDown) allPoints[face, row, row] = edgeUniquePoints[facesDefinition[face, 1], PlanetInfo.pointsPerEdge - row - 2];
                    else allPoints[face, row, row] = edgeUniquePoints[facesDefinition[face, 1], row - 1];
                }

                // Last row, the two other base points and an edge.
                // Grab the Base Point mentioned in the definiton of the third edge of this face, with Humor determining wether we take the first base point or the second (we want the j corner)

                if (flipped) allPoints[face, PlanetInfo.pointsPerEdge - 1, 0] = basePoints[edgesDefinition[facesDefinition[face, 2], 0]];
                else allPoints[face, PlanetInfo.pointsPerEdge - 1, 0] = basePoints[edgesDefinition[facesDefinition[face, 2], 0]];

                for (uint col = 1; col < PlanetInfo.pointsPerEdge - 1; col++)
                {
                    uint index = col - 1;
                    //if (flipped) index = PlanetInfo.pointsPerEdge - col - 2;

                    allPoints[face, PlanetInfo.pointsPerEdge - 1, col] = edgeUniquePoints[facesDefinition[face, 2], index];
                }

                if (flipped) allPoints[face, PlanetInfo.pointsPerEdge - 1, PlanetInfo.pointsPerEdge - 1] = basePoints[edgesDefinition[facesDefinition[face, 2], 1]];
                else allPoints[face, PlanetInfo.pointsPerEdge - 1, PlanetInfo.pointsPerEdge - 1] = basePoints[edgesDefinition[facesDefinition[face, 2], 1]];

            }

        }
        else if (depth == 1)
        {

        }
        else // depth == 0
        {
            for (int face = 0; face < 20; face++) // Faces 00 to 04, inclusive
            {
                bool upsideDown = facesDefinition[face, 3] == 1;
                bool flipped = facesDefinition[face, 4] == 1;

                // I Corner
                if (upsideDown) allPoints[face, 0, 0] = basePoints[edgesDefinition[PlanetManager.facesDefinition[face, 0], 1]];
                else allPoints[face, 0, 0] = basePoints[edgesDefinition[PlanetManager.facesDefinition[face, 0], 0]];

                // J Corner
                if (flipped)
                {
                    allPoints[face, 1, 0] = basePoints[edgesDefinition[PlanetManager.facesDefinition[face, 2], 1]]; // J
                    allPoints[face, 1, 1] = basePoints[edgesDefinition[PlanetManager.facesDefinition[face, 2], 0]]; // K

                    if (face == 6)
                    {
                        GD.Print("K Point of Sector6: ", basePoints[edgesDefinition[PlanetManager.facesDefinition[face, 2], 1]]);
                        GD.Print("JK Edge of Sector6: ", edgesDefinition[PlanetManager.facesDefinition[face, 2], 1]);
                        GD.Print("   Face of Sector6: ", PlanetManager.facesDefinition[face, 2]);
                    }
                }
                else
                {
                    allPoints[face, 1, 0] = basePoints[edgesDefinition[PlanetManager.facesDefinition[face, 2], 0]]; // J
                    allPoints[face, 1, 1] = basePoints[edgesDefinition[PlanetManager.facesDefinition[face, 2], 1]]; // K
                }
            }
        }
        
        // Normalisation
        for (byte face = 0; face < 20; face++)
        {
            for (uint row = 0; row < PlanetInfo.pointsPerEdge; row++)
            {
                for (uint col = 0; col < row + 1; col++)
                {
                    allPoints[face, row, col] = allPoints[face, row, col].Normalized();

                    /*Node3D node = (Node3D)pointTest.Instantiate();
                    AddChild(node);
                    node.GlobalPosition = allPoints[face, row, col];*/
                }
            }
        }

        // Triangulation
        for (byte face = 0; face < 20; face++)
        {
            for (TileInfo tri = new TileInfo(face, 0); tri.asperaCoords < PlanetInfo.tilesPerSector; tri.asperaCoords++)
            {
                Vector2I ICorner = new Vector2I(0, 0);
                Vector2I JCorner = new Vector2I((int)PlanetInfo.pointsPerEdge - 1, 0);
                Vector2I KCorner = new Vector2I((int)PlanetInfo.pointsPerEdge - 1, (int)PlanetInfo.pointsPerEdge - 1);

                // Figure out which ones these are
                for (byte currentDepth = 0; currentDepth < PlanetInfo.depth; currentDepth++)
                {
                    Vector2I newICorner = ICorner;
                    Vector2I newJCorner = JCorner;
                    Vector2I newKCorner = KCorner;
                    switch (tri.GetMove(currentDepth))
                    {
                        case 0:
                            // I is between J and K
                            newICorner.X = JCorner.X + (KCorner.X - JCorner.X) / 2;
                            newICorner.Y = JCorner.Y + (KCorner.Y - JCorner.Y) / 2;

                            // J is between K and I
                            newJCorner.X = ICorner.X + (KCorner.X - ICorner.X) / 2;
                            newJCorner.Y = ICorner.Y + (KCorner.Y - ICorner.Y) / 2;

                            // K is between I and J
                            newKCorner.X = JCorner.X + (ICorner.X - JCorner.X) / 2;
                            newKCorner.Y = JCorner.Y + (ICorner.Y - JCorner.Y) / 2;
                            break;
                        case 1:
                            // I does not change
                            newJCorner.X = JCorner.X + (ICorner.X - JCorner.X) / 2;
                            newJCorner.Y = JCorner.Y + (ICorner.Y - JCorner.Y) / 2;

                            newKCorner.X = KCorner.X + (ICorner.X - KCorner.X) / 2;
                            newKCorner.Y = KCorner.Y + (ICorner.Y - KCorner.Y) / 2;
                            break;
                        case 2:
                            // J does not change
                            newICorner.X = ICorner.X + (JCorner.X - ICorner.X) / 2;
                            newICorner.Y = ICorner.Y + (JCorner.Y - ICorner.Y) / 2;

                            newKCorner.X = KCorner.X + (JCorner.X - KCorner.X) / 2;
                            newKCorner.Y = KCorner.Y + (JCorner.Y - KCorner.Y) / 2;
                            break;
                        case 3:
                            // K does not change
                            newICorner.X = ICorner.X + (KCorner.X - ICorner.X) / 2;
                            newICorner.Y = ICorner.Y + (KCorner.Y - ICorner.Y) / 2;

                            newJCorner.X = JCorner.X + (KCorner.X - JCorner.X) / 2;
                            newJCorner.Y = JCorner.Y + (KCorner.Y - JCorner.Y) / 2;
                            break;
                    }
                    ICorner = newICorner;
                    JCorner = newJCorner;
                    KCorner = newKCorner;
                }
                if (PlanetManager.facesDefinition[face, 3] == 0 && PlanetManager.facesDefinition[face, 4] == 0)
                {
                    Tile current = (Tile)theGrid.GetChild(face).GetChild((int)tri.asperaCoords);
                    current.InitializeVisuals(
                        allPoints[face, ICorner.X, ICorner.Y],
                        allPoints[face, KCorner.X, KCorner.Y],
                        allPoints[face, JCorner.X, JCorner.Y]);
                }
                else if (PlanetManager.facesDefinition[face, 3] == 1 && PlanetManager.facesDefinition[face, 4] == 0)
                {
                    Tile current = (Tile)theGrid.GetChild(face).GetChild((int)tri.asperaCoords);
                    current.InitializeVisuals(
                        allPoints[face, ICorner.X, ICorner.Y],
                        allPoints[face, KCorner.X, KCorner.Y],
                        allPoints[face, JCorner.X, JCorner.Y]);
                }
                else
                {
                    Tile current = (Tile)theGrid.GetChild(face).GetChild((int)tri.asperaCoords);
                    current.InitializeVisuals(
                        allPoints[face, ICorner.X, ICorner.Y],
                        allPoints[face, JCorner.X, JCorner.Y],
                        allPoints[face, KCorner.X, KCorner.Y]);
                }

            }
        }
    }



}

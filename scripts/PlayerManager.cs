using Godot;
using System;

public partial class PlayerManager : Node
{
    private const float RayLength = 1000.0f;

    [Export]
    public Camera3D camera;
    public RayCast3D rayCast;

    public override void _Ready()
    {
        rayCast = (RayCast3D)camera.GetChild(0);
    }

    public override void _Process(double delta)
    {
        Vector2 mousePos = camera.GetViewport().GetMousePosition();
        Vector3 from = camera.ProjectRayOrigin(mousePos);
        Vector3 to = from + camera.ProjectRayNormal(mousePos) * RayLength;

        rayCast.TargetPosition = to;

        if (rayCast.IsColliding())
        {
            GD.Print("a");
            var coll = rayCast.GetCollider();
           
            if (coll.GetClass() == "MeshInstance3D")
            {
                Node a = (Node)coll;
                Tile tile = (Tile)a.GetParent().GetParent();
                tile.select();
            }
            
        }
    }

}

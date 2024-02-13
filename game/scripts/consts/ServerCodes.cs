using System.Collections.Generic;
using Godot;

public static class ServerCodes
{
    public enum Action
    {
        Spawn,
        Destroy,
        SendState,
        Update
    }

    public enum Movement
    {
        Idle,
        WalkNorth,
        WalkWest,
        WalkSouth,
        WalkEast,
        WalkNorthwest,
        WalkNortheast,
        WalkSouthwest,
        WalkSoutheast,
    }

    public static Dictionary<string, PackedScene> ModelObjs = new() {
    {"player", GD.Load<PackedScene>("res://scenes/Player.tscn")}
};
}
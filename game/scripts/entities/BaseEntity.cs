using System;
using Godot;

namespace TinyMMO.Entities;

public struct BaseAction
{
    public ServerCodes.Movement movement;
    public float ticks;
    public bool locked;
    public bool done;

    public static explicit operator BaseAction(Variant v)
    {
        var dict = (Godot.Collections.Dictionary)v;
        var hasMov = dict.TryGetValue("movement", out var movement);
        var hasTicks = dict.TryGetValue("ticks", out var ticks);
        var hasLocked = dict.TryGetValue("locked", out var locked);
        var hasDone = dict.TryGetValue("done", out var done);
        return new BaseAction
        {
            movement = hasMov ? (ServerCodes.Movement)Enum.Parse(typeof(ServerCodes.Movement), (string)movement) : ServerCodes.Movement.Idle,
            ticks = hasTicks ? (float)ticks : 0,
            locked = !hasLocked || (bool)locked,
            done = hasDone && (bool)done
        };
    }
}

public interface IBaseEntity
{
    public Vector2 Position { get; set; }
    public BaseAction Action { get; set; }
    public StringName Name { get; set; }
    public void ChangeAction(ServerCodes.Movement movement, float ticks, bool locked);
    public void ChangeAction(BaseAction action);
    public void ChangeToIdle();
    public Variant Get(StringName id);
}

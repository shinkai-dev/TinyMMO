using Godot;
using System;

public class PlayaController : KinematicBody2D
{
    [Export] public int speed = 100;



    public override void _PhysicsProcess(float delta)
    {
        if (Input.IsActionPressed("ui_right"))
            MoveAndSlide(new Vector2(speed, 0));

        if (Input.IsActionPressed("ui_left"))
            MoveAndSlide(new Vector2(-speed, 0));

        if (Input.IsActionPressed("ui_down"))
            MoveAndSlide(new Vector2(0, speed));

        if (Input.IsActionPressed("ui_up"))
            MoveAndSlide(new Vector2(0, -speed));
    }
}

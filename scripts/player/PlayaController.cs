using Godot;
using System;

public class PlayaController : KinematicBody2D
{
    [Export] public int tileSize = 16;
    [Export] public bool isMoving = false;
    public Timer timer;
    public Vector2 velocity = new Vector2();

    public override void _Ready()
    {
        timer = GetNode<Timer>("stepDelay");
    }
    public void GetInput()
    {
        velocity = new Vector2();

        if (Input.IsActionPressed("ui_right"))
            velocity.x += 1;

        if (Input.IsActionPressed("ui_left"))
            velocity.x -= 1;

        if (Input.IsActionPressed("ui_down"))
            velocity.y += 1;

        if (Input.IsActionPressed("ui_up"))
            velocity.y -= 1;

        velocity = velocity * tileSize;
        isMoving = true;
        timer.Start();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(!isMoving){
            GetInput();
            MoveAndCollide(velocity);
        }
    }
    
    public void _on_stepDelay_timeout()
    {
        isMoving = false;
    }
}

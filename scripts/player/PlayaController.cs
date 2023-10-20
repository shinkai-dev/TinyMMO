

using Godot;
using System;

public class PlayaController : KinematicBody2D
{
    [Export] public int MoveSpeed = 80; 
    [Export] public float sprintMultiplier = 1.6f;
    public Sprite sprite;
    public Vector2 velocity = new Vector2();
    public Timer stepTimer;
    public float ogStepTimer;
    public AudioStreamPlayer2D stepSound;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Playa");
        stepSound = GetNode<AudioStreamPlayer2D>("stepSound");
        stepTimer = GetNode<Timer>("stepTimer");
        ogStepTimer = stepTimer.WaitTime;
    }
    
    public void GetInput(float delta)
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

        if (velocity == Vector2.Zero)
            return;

        while(velocity.x != 0){
            sprite.FlipH = velocity.x < 0;
            break;
        }
        if(Input.IsActionPressed("sprint")){
            velocity *= sprintMultiplier;
        }
        if(stepTimer.TimeLeft == 0){
            stepSound.Play();
            stepSound.PitchScale = (float)GD.RandRange(0.8f, 1.2f);
            if(Input.IsActionPressed("sprint")){
                stepTimer.Start(ogStepTimer / sprintMultiplier);}
            else{
                stepTimer.Start(ogStepTimer);
            }
        }
        velocity = velocity * MoveSpeed;

        
    }

    public override void _PhysicsProcess(float delta){
        GetInput(delta);
        velocity = MoveAndSlide (velocity);
    }

    public void _on_stepDelay_timeout(){
        
    }
    
}


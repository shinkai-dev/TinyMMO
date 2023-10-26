

using Godot;
using System;

public class Player : KinematicBody2D
{
	[Export] public int MoveSpeed = 80;
	[Export] public float sprintMultiplier = 1.6f;
	public Sprite sprite;
	public Sprite playaEyes;
	[Puppet] public Vector2 PuppetVelocity = new Vector2();
	[Puppet] public Vector2 PuppetPosition = new Vector2();
	public Vector2 Velocity = new Vector2();
	public Godot.Color PlayaColor = new Color(0, 0, 0);
	public Godot.Color PlayaEyeColor = new Color(0, 0, 0);
	public Label playerName;
	public Timer stepDelay;
	public float prevStepDelay;
	public AudioStreamPlayer2D stepSound;

    private Health health;
    private Combat combat;

	public override void _Ready()
	{
		sprite = GetNode<Sprite>("Playa");
		playaEyes = GetNode<Sprite>("Playa/Playa-eyes");
		playerName = GetNode<Label>("PlayerName");
		PuppetPosition = Position;
		PuppetVelocity = Velocity;
		//temporario para poder diferenciar os jogadores
		PlayaColor = new Color(GD.Randf(), GD.Randf(), GD.Randf());
		PlayaEyeColor = new Color(1 - PlayaColor.r, 1 - PlayaColor.g, 1 - PlayaColor.b);
		sprite.Modulate = PlayaColor;
		sprite.SelfModulate = PlayaColor;
		playaEyes.Modulate = PlayaEyeColor;
		playaEyes.SelfModulate = PlayaEyeColor;
		playerName.Text = Name.ToString();
		playerName.Modulate = PlayaColor;
		stepSound = GetNode<AudioStreamPlayer2D>("stepSound");
		stepDelay = GetNode<Timer>("stepDelay");
		prevStepDelay = stepDelay.WaitTime;
        health = GetNode<Health>("Health");
        combat = GetNode<Combat>("Combat");
		Connect("mouse_entered", this, nameof(_on_Player_mouse_entered));
		Connect("mouse_exited", this, nameof(_on_Player_mouse_exited));
	}
	
	[Remote]
	public void MakeStepSound(){
		stepSound.Play();
	}

	private void _on_Player_mouse_entered()
	{
		playerName.Visible = true;
		GD.Print("Mouse entered");
	}

	private void _on_Player_mouse_exited()
	{
		playerName.Visible = false;
		GD.Print("Mouse exited");
	}
	public void GetInput(float delta)
	{
		Velocity = new Vector2();

		if(Input.IsActionJustPressed("ui_select")){
			combat.Attack();
		}
		if (Input.IsActionPressed("ui_right"))
			Velocity.x += 1;

		if (Input.IsActionPressed("ui_left"))
			Velocity.x -= 1;

		if (Input.IsActionPressed("ui_down"))
			Velocity.y += 1;

		if (Input.IsActionPressed("ui_up"))
			Velocity.y -= 1;
		if (Input.IsActionPressed("sprint"))
		{
			Velocity *= sprintMultiplier;
		}
		if (stepDelay.TimeLeft == 0 && Velocity != Vector2.Zero)
		{
			Rpc(nameof(MakeStepSound));
			stepSound.PitchScale = (float)GD.RandRange(0.8f, 1.2f);
			if (Input.IsActionPressed("sprint"))
			{
				stepDelay.Start(prevStepDelay / sprintMultiplier);
			}
			else
			{
				stepDelay.Start(prevStepDelay);
			}
		}
		Velocity *= MoveSpeed;
		Rset(nameof(PuppetVelocity), Velocity);
		Rset(nameof(PuppetPosition), Position);
	}

	public override void _PhysicsProcess(float delta)
	{
		if (IsNetworkMaster())
		{
			GetInput(delta);
		}
		else
		{
			Position = PuppetPosition;
			Velocity = PuppetVelocity;
		}

		sprite.FlipH = Velocity.x < 0;
		playaEyes.FlipH = Velocity.x < 0;

		MoveAndSlide(Velocity);
	}

    public override void _Process(float delta)
    {
		combat.attackCursor.Rotation = GetGlobalMousePosition().AngleToPoint(GlobalPosition);
    }
}




using Godot;
using System;

public class Player : KinematicBody2D
{
	[Export] private int MoveSpeed = 80;
	[Export] private float sprintMultiplier = 1.6f;
	[Puppet] private Vector2 PuppetVelocity = new Vector2();
	[Puppet] private Vector2 PuppetPosition = new Vector2();
	public Vector2 Velocity = new Vector2();
	private Color PlayaColor_ = new Color(1, 1, 1);
	[Remote]
	public Godot.Color PlayaColor
	{
		set
		{
			GD.Print(value);
			PlayaEyeColor = new Color(1 - value.r, 1 - value.g, 1 - value.b);
			var playaEyes = GetNode<Sprite>("Playa/Playa-eyes");
			var sprite = GetNode<Sprite>("Playa");
			var playerName = GetNode<Label>("PlayerName");
			sprite.Modulate = value;
			sprite.SelfModulate = value;
			playaEyes.Modulate = PlayaEyeColor;
			playaEyes.SelfModulate = PlayaEyeColor;
			playerName.Modulate = value;
			PlayaColor_ = value;
		}
		get { return PlayaColor_; }
	}
	private Godot.Color PlayaEyeColor_ = new Color(1, 1, 1);
	public Godot.Color PlayaEyeColor
	{
		set
		{
			PlayaEyeColor_ = value;
			var playaEyes = GetNode<Sprite>("Playa/Playa-eyes");
			playaEyes.Modulate = value;
			playaEyes.SelfModulate = value;
		}
		get { return PlayaEyeColor_; }
	}
	public Timer stepDelay;
	public float prevStepDelay;
	public AudioStreamPlayer2D stepSound;
	[Remote] private string Uid_;
	public string Uid
	{
		get { return Uid_; }
	}
	private string Nickname_ = "";
	[Remote]
	public string Nickname
	{
		set { GetNode<Label>("PlayerName").Text = value; Nickname_ = value; GD.Print(value); }
		get { return Nickname_; }
	}

	public override void _Ready()
	{
		PuppetPosition = Position;
		PuppetVelocity = Velocity;
		stepSound = GetNode<AudioStreamPlayer2D>("stepSound");
		stepDelay = GetNode<Timer>("stepDelay");
		prevStepDelay = stepDelay.WaitTime;
		Connect("mouse_entered", this, nameof(_on_Player_mouse_entered));
		Connect("mouse_exited", this, nameof(_on_Player_mouse_exited));
	}

	[Remote]
	public void MakeStepSound()
	{
		stepSound.Play();
	}

	private void _on_Player_mouse_entered()
	{
		var playerName = GetNode<Label>("PlayerName");
		playerName.Visible = true;
		GD.Print("Mouse entered");
	}

	private void _on_Player_mouse_exited()
	{
		var playerName = GetNode<Label>("PlayerName");
		playerName.Visible = false;
		GD.Print("Mouse exited");
	}
	public void GetInput(float delta)
	{
		Velocity = new Vector2();
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

		var playa = GetNode<Sprite>("Playa");
		var playaEyes = GetNode<Sprite>("Playa/Playa-eyes");
		playa.FlipH = Velocity.x < 0;
		playaEyes.FlipH = Velocity.x < 0;

		MoveAndSlide(Velocity);
	}

	public void SetData(string uid, string name, Color color)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		Rset(nameof(Uid_), uid);
		Rset(nameof(Nickname), name);
		Rset(nameof(PlayaColor), new Color(color.r, color.g, color.b));
		Uid_ = uid;
		Nickname = name;
		PlayaColor = new Color(color.r, color.g, color.b);
	}

	public void UpdateData(int sessionId, string uid, string name, Color color)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		RsetId(sessionId, nameof(Uid_), uid);
		RsetId(sessionId, nameof(Nickname), name);
		RsetId(sessionId, nameof(PlayaColor), new Color(color.r, color.g, color.b));
	}
}


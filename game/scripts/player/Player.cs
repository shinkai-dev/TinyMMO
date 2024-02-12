

using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] private int MoveSpeed = 80;
	[Export] private float sprintMultiplier = 1.6f;
	private Vector2 PuppetVelocity = new Vector2();
	private Vector2 PuppetPosition = new Vector2();
	private Color PlayaColor_ = new Color(1, 1, 1);
	public Godot.Color PlayaColor
	{
		set
		{
			GD.Print(value);
			PlayaEyeColor = new Color(1 - value.R, 1 - value.G, 1 - value.B);
			var playaEyes = GetNode<Sprite2D>("Playa/Playa-eyes");
			var sprite = GetNode<Sprite2D>("Playa");
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
			var playaEyes = GetNode<Sprite2D>("Playa/Playa-eyes");
			playaEyes.Modulate = value;
			playaEyes.SelfModulate = value;
		}
		get { return PlayaEyeColor_; }
	}
	public Timer stepDelay;
	public double prevStepDelay;
	public AudioStreamPlayer2D stepSound;
	private string Uid_;
	public string Uid
	{
		get { return Uid_; }
	}
	private string Nickname_ = "";
	public string Nickname
	{
		set { GetNode<Label>("PlayerName").Text = value; Nickname_ = value; GD.Print(value); }
		get { return Nickname_; }
	}
	private bool Active_ = false;
	public bool Active
	{
		set { Active_ = value; Visible = value; }
		get { return Active_; }
	}

	public override void _Ready()
	{
		PuppetPosition = Position;
		PuppetVelocity = Velocity;
		stepSound = GetNode<AudioStreamPlayer2D>("stepSound");
		stepDelay = GetNode<Timer>("stepDelay");
		prevStepDelay = stepDelay.WaitTime;
		Connect("mouse_entered", new Callable(this, nameof(_on_Player_mouse_entered)));
		Connect("mouse_exited", new Callable(this, nameof(_on_Player_mouse_exited)));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
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
	public void GetInput(double delta)
	{
		if (!Active) {
			return;
		}

		if (Input.IsActionPressed("ui_right"))
			Velocity += new Vector2(1, 0);

		if (Input.IsActionPressed("ui_left"))
			Velocity += new Vector2(-1, 0);

		if (Input.IsActionPressed("ui_down"))
			Velocity += new Vector2(0, 1);

		if (Input.IsActionPressed("ui_up"))
			Velocity += new Vector2(0, -1);
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
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Active) {
			return;
		}

		if (IsMultiplayerAuthority())
		{
			GetInput(delta);
		}
		else
		{
			Position = PuppetPosition;
			Velocity = PuppetVelocity;
		}

		var playa = GetNode<Sprite2D>("Playa");
		var playaEyes = GetNode<Sprite2D>("Playa/Playa-eyes");
		playa.FlipH = Velocity.X < 0;
		playaEyes.FlipH = Velocity.Y < 0;

		MoveAndSlide();
	}

	public void SetData(string uid, string name, Color color, bool active)
	{
		if (Multiplayer.GetUniqueId() != 1)
		{
			return;
		}

		Uid_ = uid;
		Nickname = name;
		PlayaColor = new Color(color.R, color.G, color.B);
		Active = active;
	}

	public void UpdateData(int sessionId, string uid, string name, Color color, bool active)
	{
		if (Multiplayer.GetUniqueId() != 1)
		{
			return;
		}
	}
}


using Godot;
using System;

public class CreateCharMenu : Node
{
	private PopupController PopupController;
	private LineEdit PlayerName;
	private Button Create;
	private AuthController AuthController;
	[Signal] public delegate bool Done();

	public override void _Ready()
	{
		PopupController = GetNode<PopupController>("/root/PopupController");
		PlayerName = GetNode<LineEdit>("Name");
		Create = GetNode<Button>("Create");
		AuthController = GetNode<AuthController>("/root/AuthController");

		Create.Connect("pressed", this, nameof(OnCreatePressed));
	}

	async void OnCreatePressed()
	{
		var name = GetNode<LineEdit>("Name").Text.Trim();
		if (name.Length < 3)
		{
			_ = PopupController.ShowMessage("Error", "The name should have three characters or more");
			return;
		}
		AuthController.CreateCharacter(name);
		PopupController.ShowLoading();
		ToggleForm(false);
		await ToSignal(PopupController, nameof(PopupController.PopupToggled));
		EmitSignal(nameof(Done), true);
		Owner.QueueFree();
	}

	void ToggleForm(bool enabled)
	{
		PlayerName.Editable = enabled;
		Create.Disabled = !enabled;
	}

}

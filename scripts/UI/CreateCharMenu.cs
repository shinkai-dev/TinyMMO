using Godot;
using System;

public class CreateCharMenu : Node
{
	private PopupController PopupController;
	private LineEdit PlayerName;
	private Button Create;
	private Button Return;
	private AuthController AuthController;

	public override void _Ready()
	{
		PopupController = GetNode<PopupController>("/root/PopupController");
		PlayerName = GetNode<LineEdit>("Name");
		Create = GetNode<Button>("Create");
		Return = GetNode<Button>("Return");
		AuthController = GetNode<AuthController>("/root/AuthController");

		Create.Connect("pressed", this, nameof(OnCreatePressed));
		Return.Connect("pressed", this, nameof(OnReturnPressed));
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
		ToggleForm(true);
	}

	void ToggleForm(bool enabled)
	{
		PlayerName.Editable = enabled;
		Create.Disabled = !enabled;
		Return.Disabled = !enabled;
	}

	void OnReturnPressed()
	{
		GetTree().ChangeScene("res://scenes/menu/menuPrin.tscn");
	}
}

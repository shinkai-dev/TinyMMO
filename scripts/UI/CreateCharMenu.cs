using Godot;
using System;

public class CreateCharMenu : Node
{
	private PopupController PopupController;

	public override void _Ready()
	{
		PopupController = GetNode<PopupController>("/root/PopupController");

		GetNode<Button>("Create").Connect("pressed", this, nameof(OnCreatePressed));
	}

	void OnCreatePressed()
	{
		var name = GetNode<LineEdit>("Name").Text.Trim();
		if (name.Length < 3)
		{
			_ = PopupController.ShowMessage("Error", "The name should have three characters or more");
			return;
		}
		GetNode<AuthController>("/root/AuthController").CreateCharacter(name);
	}
}

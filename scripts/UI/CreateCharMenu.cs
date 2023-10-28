using Godot;
using System;

public class CreateCharMenu : Node
{
	public override void _Ready()
	{
		GetNode<Button>("Create").Connect("pressed", this, nameof(OnCreatePressed));
	}

	void OnCreatePressed()
	{
		var name = GetNode<LineEdit>("Name").Text.Trim();
		if (name.Length < 3)
		{
			var gamePopupScene = (PackedScene)ResourceLoader.Load("res://scenes/menu/GamePopup.tscn");
			var gamePopup = (AcceptDialog)gamePopupScene.Instance();
			gamePopup.WindowTitle = "Error";
			gamePopup.DialogText = "The name should have three characters or more";
			GetTree().Root.AddChild(gamePopup);
			gamePopup.Show();
			return;
		}
		GetNode<AuthController>("/root/AuthController").CreateCharacter(name);
	}
}

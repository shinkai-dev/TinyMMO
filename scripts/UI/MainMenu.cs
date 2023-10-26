using Godot;
using System;

public class MainMenu : Node
{
	public override void _Ready()
	{
		GetNode<Button>("Play").Connect("pressed", this, nameof(OnPlayPressed));
		GetNode<Button>("Host").Connect("pressed", this, nameof(OnHostPressed));
		GetNode<Button>("Register").Connect("pressed", this, nameof(OnRegisterPressed));
	}

	void OnPlayPressed() {
		var email = GetNode<LineEdit>("Email").Text;
		var password = GetNode<LineEdit>("Password").Text;
		GetNode<AuthController>("/root/AuthController").Login(email, password);
	}

	void OnHostPressed()
	{
		var peer = new NetworkedMultiplayerENet();
		peer.CreateServer(NetworkConsts.PORT, NetworkConsts.MAX_PLAYERS);
		GetTree().NetworkPeer = peer;
	
		OpenGame();
	}
	
	void OnRegisterPressed() {
		GetTree().ChangeScene("res://scenes/menu/Register.tscn");
	}

	void OpenGame() {
		GetTree().ChangeScene("res://scenes/GameController.tscn");
	}
}

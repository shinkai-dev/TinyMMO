using Godot;
using System;

public class MainMenu : Node
{
	public override void _Ready()
	{
		GetNode<Button>("Play").Connect("pressed", this, nameof(OnPlayPressed));
		GetNode<Button>("Host").Connect("pressed", this, nameof(OnHostPressed));
	}

	void OnPlayPressed() {
		var peer = new NetworkedMultiplayerENet();
		peer.CreateClient(NetworkConsts.IP, NetworkConsts.PORT);
		GetTree().NetworkPeer = peer;
	
		OpenGame();
	}

	void OnHostPressed()
	{
		var peer = new NetworkedMultiplayerENet();
		peer.CreateServer(NetworkConsts.PORT, NetworkConsts.MAX_PLAYERS);
		GetTree().NetworkPeer = peer;
	
		OpenGame();
	}
	
	void OpenGame() {
		GetTree().ChangeScene("res://scenes/GameController.tscn");
	}
}

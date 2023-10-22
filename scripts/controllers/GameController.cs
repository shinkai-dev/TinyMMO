using Godot;
using System;
using System.Collections.Generic;

public class GameController : Node
{
	private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();

	public override void _Ready()
	{
		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
		GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connection_failed", this, nameof(ServerDisconnected));
		GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
	}

	void PlayerConnected(int id) {
		GD.Print("Player connected: " + id);
		if (id > 1)
			AddPlayer(id);
	}

	void PlayerDisconnected(int id) {
		GD.Print("Player disconnected: " + id);
		players[id].QueueFree();
		players.Remove(id);
	}

	void ConnectedToServer() {
		GD.Print("Connected to server");
		AddPlayer(GetTree().GetNetworkUniqueId());
	}

	void ServerDisconnected() {
		GD.Print("Server disconnected");
		GetTree().ChangeScene("res://scenes/MainMenu.tscn");
	}

	void AddPlayer(int id) {
		GD.Print("Add player: " + id);
		var newPlayer = (PackedScene)ResourceLoader.Load("res://scenes/Player.tscn");
		var playerInstance = (Player)newPlayer.Instance();
		var spawnPoint = GetNode<Node2D>("World/Spawnpoint").GlobalPosition;
		playerInstance.Name = id + "";
		playerInstance.SetNetworkMaster(id);
		playerInstance.GlobalPosition = spawnPoint;
		players[id] = playerInstance;
		AddChild(playerInstance);
	}
}

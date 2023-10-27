using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

public class GameController : Node
{
	private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();
	private readonly HttpClient client = new HttpClient();

	public override void _Ready()
	{
		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
		GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connection_failed", this, nameof(ServerDisconnected));
		GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
		GD.Print(GetTree().GetNetworkUniqueId());
	}

	void PlayerConnected(int id)
	{
		GD.Print("Player connected: " + id);
		if (id > 1)
			AddPlayer(id);
	}

	void PlayerDisconnected(int id)
	{
		GD.Print("Player disconnected: " + id);
		players[id].QueueFree();
		players.Remove(id);
	}

	void ConnectedToServer()
	{
		GD.Print("Connected to server");
		AddPlayer(GetTree().GetNetworkUniqueId());
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			RpcId(1, nameof(CreateUser), GetNode<AuthController>("/root/AuthController").GetToken(), "carlos");
		}
	}

	void ServerDisconnected()
	{
		GD.Print("Server disconnected");
		GetTree().ChangeScene("res://scenes/MainMenu.tscn");
	}

	void AddPlayer(int id)
	{
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

	[Master]
	public void CreateUser(string authToken, string name)
	{
		GD.Print(name);
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		var tokenCheckRequest = new HTTPRequest();
		tokenCheckRequest.Connect("request_completed", this, nameof(CreateUserOnTokenChecked), new Godot.Collections.Array() { name });
		AddChild(tokenCheckRequest);
		GD.Print(authToken);
		var body = JSON.Print(
			new Godot.Collections.Dictionary()
			{
				{ "grant_type", "refresh_token" },
				{ "refresh_token", authToken },
			}
		);
		var headers = new string[]
		{
			"Content-Type: application/json",
			"Content-Length: " + body.Length
		};
		tokenCheckRequest.Request(EndpointConsts.TOKEN_CHECK, headers, true, HTTPClient.Method.Post, body);
	}

	public void CreateUserOnTokenChecked(int result, int responseCode, string[] headers, byte[] body, string name)
	{
		var response = JSON.Parse(Encoding.UTF8.GetString(body)).Result as Godot.Collections.Dictionary;
		var uid = response["user_id"];
		GD.Print(name);
		var userCollection = new UserCollection();
		userCollection.SetDoc(
			new UserModel()
			{
				Id = uid + "",
				Name = name
			}
		);
	}
}

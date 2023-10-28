using Godot;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

public class GameController : Node
{
	private readonly Dictionary<int, Player> PlayersPerSession = new Dictionary<int, Player>();
	private readonly Dictionary<string, Player> PlayersPerUid = new Dictionary<string, Player>();
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

	[Master]
	void RegisterPlayer(int sessionId, string playerUid, string nickname, Color color) {
		AddPlayer(sessionId, playerUid, nickname, color);
	}

	void PlayerConnected(int id)
	{
		GD.Print("Player connected: " + id);
		// if (id > 1)
		// 	AddPlayer(id);
	}

	void PlayerDisconnected(int id)
	{
		GD.Print("Player disconnected: " + id);
		if (!PlayersPerSession.ContainsKey(id))
		{
			return;
		}
		PlayersPerSession[id].QueueFree();
		PlayersPerSession.Remove(id);
		PlayersPerUid.Remove(PlayersPerSession[id].uid);
	}

	void ConnectedToServer()
	{
		GD.Print("Connected to server");
		var id = GetTree().GetNetworkUniqueId();
		SetNetworkMaster(id);
		if (id != 1)
		{
			RpcId(1, nameof(GetUserByToken), id, GetNode<AuthController>("/root/AuthController").GetToken());
		}
	}

	void ServerDisconnected()
	{
		GD.Print("Server disconnected");
		GetTree().ChangeScene("res://scenes/MainMenu.tscn");
	}

	void AddPlayer(int id, string playerUid, string name, Color color)
	{
		GD.Print("Add player: " + id);
		var newPlayer = (PackedScene)ResourceLoader.Load("res://scenes/Player.tscn");
		var playerInstance = (Player)newPlayer.Instance();
		var spawnPoint = GetNode<Node2D>("World/Spawnpoint").GlobalPosition;
		playerInstance.Name = id + "";
		playerInstance.SetNetworkMaster(id);
		playerInstance.GlobalPosition = spawnPoint;
		playerInstance.uid = playerUid;
		playerInstance.Nickname = name;
		playerInstance.PlayaColor = color;
		PlayersPerSession[id] = playerInstance;
		PlayersPerUid[playerUid] = playerInstance;
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

		public void GetTokenForUser(int result, int responseCode, string[] headers, byte[] body, int sessionId)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		var response = JSON.Parse(Encoding.UTF8.GetString(body)).Result as Godot.Collections.Dictionary;
		GD.Print(response);
		var uid = response["user_id"] as string;
		if (PlayersPerUid.ContainsKey(uid))
		{
			var player = PlayersPerUid[uid];
			RpcId(sessionId, nameof(RegisterPlayer), player.GetNetworkMaster(), uid, player.Name, player.PlayaColor);
		}
		else
		{
			var userCol = new UserCollection();
			var playerModel = userCol.GetDoc(uid);
			if (playerModel != null)
			{
				var player = (PackedScene)ResourceLoader.Load("res://scenes/Player.tscn");
				var playerInstance = (Player)player.Instance();
				playerInstance.Name = sessionId + "";
				playerInstance.SetNetworkMaster(sessionId);
				playerInstance.GlobalPosition = GetNode<Node2D>("World/Spawnpoint").GlobalPosition;
				playerInstance.uid = uid;
				playerInstance.Nickname = playerModel.Name;
				PlayersPerSession[sessionId] = playerInstance;
				PlayersPerUid[uid] = playerInstance;
				playerInstance.PlayaColor = new Color(playerModel.Color.r, playerModel.Color.g, playerModel.Color.b);
				AddChild(playerInstance);
				RpcId(sessionId, nameof(RegisterPlayer), playerInstance.GetNetworkMaster(), uid, playerInstance.Nickname, playerInstance.PlayaColor);
			}
			else
			{
				RpcId(sessionId, nameof(SendToCharacterCreate));
			}
		}
	}

	[Master]
	void SendToCharacterCreate() {
		GetTree().ChangeScene("res://scenes/menu/CreateCharacter.tscn");
	}

	[Master]
	public void GetUserByToken(int id, string token)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		var tokenCheckRequest = new HTTPRequest();
		tokenCheckRequest.Connect("request_completed", this, nameof(GetTokenForUser), new Godot.Collections.Array() { id });
		AddChild(tokenCheckRequest);
		var body = JSON.Print(
			new Godot.Collections.Dictionary()
			{
				{ "grant_type", "refresh_token" },
				{ "refresh_token", token },
			}
		);
		var headers = new string[]
		{
			"Content-Type: application/json",
			"Content-Length: " + body.Length
		};
		tokenCheckRequest.Request(EndpointConsts.TOKEN_CHECK, headers, true, HTTPClient.Method.Post, body);
	}

	public void GetUserForId(int id, Godot.Collections.Dictionary user)
	{
		GD.Print(user);
		RpcId(id, nameof(CreateUser), user["id"], user["name"]);
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

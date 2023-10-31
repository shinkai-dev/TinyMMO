using Godot;
using System.Collections.Generic;

public class GameController : Node
{
	private Dictionary<int, Player> PlayersPerSession = new Dictionary<int, Player>();
	private readonly Dictionary<string, Player> PlayersPerUid = new Dictionary<string, Player>();
	private PopupController	PopupController;

	public override void _Ready()
	{
		PopupController = GetNode<PopupController>("/root/PopupController");

		GetTree().Connect("connected_to_server", this, nameof(ConnectedToServer));
		GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("connection_failed", this, nameof(ServerDisconnected));
		GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected));
	}

	void PlayerConnected(int id)
	{
		GD.Print("Player connected: " + id);
		if (PlayersPerSession.ContainsKey(id) || id == 1)
		{
			return;
		}
		AddPlayer(id);
	}

	void PlayerDisconnected(int id)
	{
		GD.Print("Player disconnected: " + id);
		if (!PlayersPerSession.ContainsKey(id))
		{
			return;
		}
		if (PlayersPerUid.ContainsKey(PlayersPerSession[id].Uid))
		{
			PlayersPerUid.Remove(PlayersPerSession[id].Uid);
		}
		PlayersPerSession[id].QueueFree();
		PlayersPerSession.Remove(id);
	}

	void ConnectedToServer()
	{
		GD.Print("Connected to server");
		var id = GetTree().GetNetworkUniqueId();
		if (id != 1)
		{
			AddPlayer(id);
			RpcId(1, nameof(GetUserByToken), id, GetNode<AuthController>("/root/AuthController").GetToken());
		}
	}

	void ServerDisconnected()
	{
		GD.Print("Server disconnected");
		GetTree().ChangeScene("res://scenes/MainMenu.tscn");
	}

	Player AddPlayer(int id)
	{
		GD.Print("Add player: " + id);
		var newPlayer = (PackedScene)ResourceLoader.Load("res://scenes/Player.tscn");
		var playerInstance = (Player)newPlayer.Instance();
		var spawnPoint = GetNode<Node2D>("World/Spawnpoint").GlobalPosition;
		playerInstance.Name = id + "";
		playerInstance.SetNetworkMaster(id);
		playerInstance.GlobalPosition = spawnPoint;
		PlayersPerSession[id] = playerInstance;
		AddChild(playerInstance);
		return playerInstance;
	}

	[Master]
	public async void CreateUser(string authToken, string name)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		var body =
			new Godot.Collections.Dictionary()
			{
				{ "grant_type", "refresh_token" },
				{ "refresh_token", authToken },
			};
		var httpController = GetNode<HttpController>("/root/HttpController");
		var response = await httpController.Post(EndpointConsts.TOKEN_CHECK, body);
		var uid = response.Body["user_id"];
		var userCollection = new UserCollection();
		userCollection.SetDoc(
			new UserModel()
			{
				Id = uid + "",
				Name = name
			}
		);
	}

	[Master]
	void SendToCharacterCreate()
	{
		GetTree().ChangeScene("res://scenes/menu/CreateCharacter.tscn");
	}

	[Master]
	async void AlreadyConnected()
	{
		await PopupController.ShowMessage("Error", "You are already connected");
		GetTree().ChangeScene("res://scenes/MainMenu.tscn");
	}

	[Master]
	public async void GetUserByToken(int id, string token)
	{
		if (GetTree().GetNetworkUniqueId() != 1)
		{
			return;
		}

		var body = new Godot.Collections.Dictionary()
			{
				{ "grant_type", "refresh_token" },
				{ "refresh_token", token },
			};
		var httpController = GetNode<HttpController>("/root/HttpController");
		var response = await httpController.Post(EndpointConsts.TOKEN_CHECK, body);
		var uid = response.Body["user_id"] as string;

		if (PlayersPerUid.ContainsKey(uid))
		{
			RpcId(id, nameof(AlreadyConnected));
			return;
		}

		var userCol = new UserCollection();
		var playerModel = userCol.GetDoc(uid);
		if (playerModel != null)
		{
			var player = PlayersPerSession[id];
			var color = new Color()
			{
				r = playerModel.Color.r,
				g = playerModel.Color.g,
				b = playerModel.Color.b
			};
			player.SetData(playerModel.Id, playerModel.Name, color);
			PlayersPerUid[uid] = player;
		}
		else
		{
			RpcId(id, nameof(SendToCharacterCreate));
			return;
		}
		foreach (var player in PlayersPerSession)
		{
			if (player.Key == id)
			{
				continue;
			}
			var playerVal = player.Value;
			var playerColor = playerVal.PlayaColor;
			var color = new Color()
			{
				r = playerVal.PlayaColor.r,
				g = playerVal.PlayaColor.g,
				b = playerVal.PlayaColor.b
			};
			playerVal.UpdateData(id, playerVal.Uid, playerVal.Nickname, color);
		}
	}
}

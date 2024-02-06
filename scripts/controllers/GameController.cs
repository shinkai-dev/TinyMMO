using Godot;
using System.Collections.Generic;

public partial class GameController : Node
{
	private Dictionary<int, Player> PlayersPerSession = new Dictionary<int, Player>();
	private readonly Dictionary<string, Player> PlayersPerUid = new Dictionary<string, Player>();
	private PopupController PopupController;

	public override void _Ready()
	{
		PopupController = GetNode<PopupController>("/root/PopupController");

		GetTree().Connect("connected_to_server", new Callable(this, nameof(ConnectedToServer)));
		GetTree().Connect("network_peer_connected", new Callable(this, nameof(PlayerConnected)));
		GetTree().Connect("network_peer_disconnected", new Callable(this, nameof(PlayerDisconnected)));
		GetTree().Connect("connection_failed", new Callable(this, nameof(ServerDisconnected)));
		GetTree().Connect("server_disconnected", new Callable(this, nameof(ServerDisconnected)));
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
		var player = PlayersPerSession[id];
		if (player.Uid != null && PlayersPerUid.ContainsKey(player.Uid))
		{
			PlayersPerUid.Remove(player.Uid);
		}
		player.QueueFree();
		PlayersPerSession.Remove(id);
	}

	void ConnectedToServer()
	{
		GD.Print("Connected to server");
		var id = GetTree().GetUniqueId();
		if (id != 1)
		{
			AddPlayer(id);
			RpcId(
				1,
				nameof(GetUserByToken),
				id,
				GetNode<AuthController>("/root/AuthController").GetToken()
			);
		}
	}

	void ServerDisconnected()
	{
		GD.Print("Server disconnected");
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}

	Player AddPlayer(int id)
	{
		GD.Print("Add player: " + id);
		var newPlayer = (PackedScene)ResourceLoader.Load("res://scenes/Player.tscn");
		var playerInstance = (Player)newPlayer.Instance();
		var spawnPoint = GetNode<Node2D>("World/Spawnpoint").GlobalPosition;
		playerInstance.Name = id + "";
		playerInstance.SetMultiplayerAuthority(id);
		playerInstance.GlobalPosition = spawnPoint;
		PlayersPerSession[id] = playerInstance;
		AddChild(playerInstance);
		return playerInstance;
	}

	The master and mastersync rpc behavior is not officially supported anymore. Try using another keyword or making custom logic using Multiplayer.GetRemoteSenderId()
[RPC]
	public async void CreateUser(string authToken, string name)
	{
		if (GetTree().GetUniqueId() != 1)
		{
			return;
		}

		var body = new Godot.Collections.Dictionary()
		{
			{ "grant_type", "refresh_token" },
			{ "refresh_token", authToken },
		};
		var httpController = GetNode<HttpController>("/root/HttpController");
		var response = await httpController.Post(EndpointConsts.TOKEN_CHECK, body);
		var uid = response.Body["user_id"];
		var userCollection = new UserCollection();
		userCollection.SetDoc(new UserModel() { Id = uid + "", Name = name });
	}

	[RPC]
	async void SendToCharacterCreate()
	{
		var CharacterCreationMenu = (PackedScene)ResourceLoader.Load("res://scenes/menu/CreateCharacter.tscn");
		var CharacterCreationMenuInstance = (Popup)CharacterCreationMenu.Instance();
		GetTree().Root.GetNode("ContainerCanvas").AddChild(CharacterCreationMenuInstance);
		var menu = CharacterCreationMenuInstance.GetNode("CenterContainer").GetNode("Menu");
		CharacterCreationMenuInstance.PopupCentered();
		CharacterCreationMenuInstance.Raise();
		await ToSignal(menu, nameof(CreateCharMenu.Done));
		var id = GetTree().GetUniqueId();
		RpcId(1, nameof(GetUserByToken), id, GetNode<AuthController>("/root/AuthController").GetToken());
	}

	[RPC]
	async void AlreadyConnected()
	{
		GetTree().ChangeSceneToFile("res://scenes/menu/menuPrin.tscn");
		await PopupController.ShowMessage("Error", "You are already connected");
	}

	The master and mastersync rpc behavior is not officially supported anymore. Try using another keyword or making custom logic using Multiplayer.GetRemoteSenderId()
[RPC]
	public async void GetUserByToken(int id, string token)
	{
		if (GetTree().GetUniqueId() != 1)
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
			player.SetData(playerModel.Id, playerModel.Name, color, true);
			PlayersPerUid[uid] = player;
		}
		else
		{
			RpcId(id, nameof(SendToCharacterCreate));
		}
		foreach (var playerChar in PlayersPerSession)
		{
			if (playerChar.Key == id)
			{
				continue;
			}
			var playerVal = playerChar.Value;
			var playerColor = playerVal.PlayaColor;
			var playerNativeColor = new Color()
			{
				r = playerVal.PlayaColor.r,
				g = playerVal.PlayaColor.g,
				b = playerVal.PlayaColor.b
			};
			playerVal.UpdateData(id, playerVal.Uid, playerVal.Nickname, playerNativeColor, true);
		}
	}
}

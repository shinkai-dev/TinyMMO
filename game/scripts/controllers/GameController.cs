using System.Collections.Generic;
using Godot;
using TinyMMO.Entities;

namespace TinyMMO.Controllers;

public partial class GameController : Node
{
    private float TickSecs = (float)ProjectSettings.GetSetting("server/tick_rate");
    private readonly string Ip = (string)ProjectSettings.GetSetting("server/ip");
    private readonly WebSocketPeer Socket = new();
    private bool FirstPoll = true;
    private readonly Queue<string> MessageList = new();

    public override void _Ready()
    {
        Socket.ConnectToUrl(Ip);
    }

    public override void _Process(double delta)
    {
        TickSecs -= (float)delta;
        Socket.Poll();
        var state = Socket.GetReadyState();
        if (state == WebSocketPeer.State.Open)
        {
            var message = MessageList.Count > 0 ? MessageList.Dequeue() : null;
            _ = message == null || TickSecs > 0 ?
                Socket.Send(System.Array.Empty<byte>()) :
                Socket.SendText(message);
            if (FirstPoll)
            {
                ProjectSettings.SetSetting("server/client_id", Socket.GetPacket().GetStringFromAscii());
                FirstPoll = false;
            }

            while (Socket.GetAvailablePacketCount() > 0)
            {
                var res = Socket.GetPacket().GetStringFromAscii();
                //ascii converstion
                var action = res[0] - 48;
                res = res[1..];
                switch (action)
                {
                    case (int)ServerCodes.Action.Spawn: Spawn(res); break;
                    case (int)ServerCodes.Action.SendState: LoadGameState(res); break;
                    case (int)ServerCodes.Action.Destroy: DeleteObj(res); break;
                    case (int)ServerCodes.Action.Update: UpdateObj(res); break;
                }
            }
        }
        else if (state == WebSocketPeer.State.Closed)
        {
            var code = Socket.GetCloseCode();
            var reason = Socket.GetCloseReason();
            GD.Print($"Connection closed with code {code} and reason {reason}");
            SetProcess(false);
        }

    }

    public static void StateToInstance(Godot.Collections.Dictionary<string, Variant> state, IBaseEntity instance)
    {
        GD.Print(instance);
        if (instance is null)
            return;
        foreach (var kvp in state)
        {
            var key = kvp.Key;
            var value = kvp.Value;
            GD.Print(key);

            switch (key)
            {
                case "id":
                    instance.Name = value.ToString();
                    break;
                default:
                    if ((object)instance.Get(key) is not null)
                    {
                        BaseAction action = (BaseAction)instance.Get("action");
                        switch (key)
                        {
                            case "transform":
                                if (action.locked == true)
                                    return;

                                instance.Position = new Vector2(
                                    ((Vector2)value).X,
                                    ((Vector2)value).Y
                                );
                                break;
                            case "action":
                                instance.ChangeAction(action);
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public void Spawn(string res)
    {
        var objState = Json.ParseString(res).AsGodotDictionary<string, Variant>();
        IBaseEntity objInstance = (IBaseEntity)ServerCodes.ModelObjs[objState["model"].ToString().ToLower()].Instantiate();
        StateToInstance(objState, objInstance);
        AddChild((Node)objInstance);
    }

    public void LoadGameState(string res)
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);
            RemoveChild(child);
            child.QueueFree();
        }
        var state = Json.ParseString(res).AsGodotDictionary<string, Variant>();
        foreach (var kvp in state)
        {
            Spawn(kvp.Value.ToString());
        }
    }

    public void DeleteObj(string res)
    {
        RemoveChild(GetNode(res));
    }

    public void UpdateObj(string res)
    {
        var objState = Json.ParseString(res).AsGodotDictionary<string, Variant>();
        var objInstance = GetNode(objState["id"].ToString());
        StateToInstance(objState, objInstance as IBaseEntity);
    }
}

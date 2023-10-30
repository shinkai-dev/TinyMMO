#if DISCORD
using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

[System.Serializable]
public class DiscordEvent<T>
{
	public Action<T> e;
	
	public DiscordEvent(Action<T> e)
	{
		this.e = e;
	}
	
	public void Invoke(T arg)
	{
		e.Invoke(arg);
	}
	
	public List<Action> actions = new List<Action>();
	
	public void AddListener(Action action)
	{
		actions.Add(action);
	}
	
	public void RemoveListener(Action action)
	{
		actions.Remove(action);
	}
}

public class Discord : Node
{
	public static Discord discord;
	
	public DiscordRpc.RichPresence presence;
	public string applicationId = "1168348162306625600";
	public string optionalSteamId;
	public int clickCounter;
	public DiscordRpc.DiscordUser joinRequest;

	DiscordRpc.EventHandlers handlers;

	public void RequestRespondYes()
	{
		GD.Print("Discord: responding yes to Ask to Join request");
		DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.Yes);
	}

	public void RequestRespondNo()
	{
		GD.Print("Discord: responding no to Ask to Join request");
		DiscordRpc.Respond(joinRequest.userId, DiscordRpc.Reply.No);
	}

	public void ReadyCallback(ref DiscordRpc.DiscordUser connectedUser)
	{
		GD.Print(string.Format("Discord: connected to {0}#{1}: {2}", connectedUser.username, connectedUser.discriminator, connectedUser.userId));
	}

	public void DisconnectedCallback(int errorCode, string message)
	{
		GD.Print(string.Format("Discord: disconnect {0}: {1}", errorCode, message));
	}

	public void ErrorCallback(int errorCode, string message)
	{
		GD.Print(string.Format("Discord: error {0}: {1}", errorCode, message));
	}

	public void JoinCallback(string secret)
	{
		GD.Print(string.Format("Discord: join ({0})", secret));
	}

	public void SpectateCallback(string secret)
	{
		GD.Print(string.Format("Discord: spectate ({0})", secret));
	}

	public void RequestCallback(ref DiscordRpc.DiscordUser request)
	{
		GD.Print(string.Format("Discord: join request {0}#{1}: {2}", request.username, request.discriminator, request.userId));
		joinRequest = request;
	}

	public override void _EnterTree()
	{
		discord = this;
	}

	public override void _Ready()
	{
		handlers = new DiscordRpc.EventHandlers();
		handlers.readyCallback += ReadyCallback;
		handlers.disconnectedCallback += DisconnectedCallback;
		handlers.errorCallback += ErrorCallback;
		handlers.joinCallback += JoinCallback;
		handlers.spectateCallback += SpectateCallback;
		handlers.requestCallback += RequestCallback;
		try {
			DiscordRpc.Initialize(applicationId, ref handlers, true, optionalSteamId);
			presence = new DiscordRpc.RichPresence();
			UpdatePresence();
		}
		catch(DllNotFoundException) {}
	}

	public override void _ExitTree()
	{
		if (presence != null) DiscordRpc.Shutdown();
	}
	
	public override void _Process(float delta)
	{
		if (presence != null)
		{
			DiscordRpc.RunCallbacks();
		}
	}
	
	// TODO: This must be run manually. IDK if you can put this in Process without lagging
	public void UpdatePresence()
	{
		if (presence == null) return;
		
		// TODO: Implement your game specific stuff here
		presence.details = "Main Menu";
		presence.startTimestamp = 0;
		presence.state = "";
		presence.partyId = "";
		presence.partySize = 0;
		presence.partyMax = 0;
		presence.joinSecret = "";	
		
		DiscordRpc.UpdatePresence(presence);
	}
}
	Discord discord = new Discord();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		discord.presence = new DiscordRpc.RichPresence()
		{
			state = "No Menu"
		};
		discord.UpdatePresence();
	}
public class DiscordRpc
{
#if NATIVE_WIN64
	const string path = "Natives/discord-rpc/WIN64/discord-rpc.dll";
#elif NATIVE_OSX64
	const string path = "Natives/discord-rpc/OSX64/libdiscord-rpc.dylib";
#elif NATIVE_LIN64
	const string path = "Natives/discord-rpc/LIN64/libdiscord-rpc.so";
#else
	const string path = ""; // OS is unsupported!
#endif
	
	//[MonoPInvokeCallback(typeof(OnReadyInfo))]
	public static void ReadyCallback(ref DiscordUser connectedUser) { Callbacks.readyCallback(ref connectedUser); }
	public delegate void OnReadyInfo(ref DiscordUser connectedUser);

	//[MonoPInvokeCallback(typeof(OnDisconnectedInfo))]
	public static void DisconnectedCallback(int errorCode, string message) { Callbacks.disconnectedCallback(errorCode, message); }
	public delegate void OnDisconnectedInfo(int errorCode, string message);

	//[MonoPInvokeCallback(typeof(OnErrorInfo))]
	public static void ErrorCallback(int errorCode, string message) { Callbacks.errorCallback(errorCode, message); }
	public delegate void OnErrorInfo(int errorCode, string message);

	//[MonoPInvokeCallback(typeof(OnJoinInfo))]
	public static void JoinCallback(string secret) { Callbacks.joinCallback(secret); }
	public delegate void OnJoinInfo(string secret);

	//[MonoPInvokeCallback(typeof(OnSpectateInfo))]
	public static void SpectateCallback(string secret) { Callbacks.spectateCallback(secret); }
	public delegate void OnSpectateInfo(string secret);

	//[MonoPInvokeCallback(typeof(OnRequestInfo))]
	public static void RequestCallback(ref DiscordUser request) { Callbacks.requestCallback(ref request); }
	public delegate void OnRequestInfo(ref DiscordUser request);

	static EventHandlers Callbacks { get; set; }

	public struct EventHandlers
	{
		public OnReadyInfo readyCallback;
		public OnDisconnectedInfo disconnectedCallback;
		public OnErrorInfo errorCallback;
		public OnJoinInfo joinCallback;
		public OnSpectateInfo spectateCallback;
		public OnRequestInfo requestCallback;
	}

	[Serializable, StructLayout(LayoutKind.Sequential)]
	public struct RichPresenceStruct
	{
		public IntPtr state; /* max 128 bytes */
		public IntPtr details; /* max 128 bytes */
		public long startTimestamp;
		public long endTimestamp;
		public IntPtr largeImageKey; /* max 32 bytes */
		public IntPtr largeImageText; /* max 128 bytes */
		public IntPtr smallImageKey; /* max 32 bytes */
		public IntPtr smallImageText; /* max 128 bytes */
		public IntPtr partyId; /* max 128 bytes */
		public int partySize;
		public int partyMax;
		public IntPtr matchSecret; /* max 128 bytes */
		public IntPtr joinSecret; /* max 128 bytes */
		public IntPtr spectateSecret; /* max 128 bytes */
		public bool instance;
	}

	[Serializable]
	public struct DiscordUser
	{
		public string userId;
		public string username;
		public string discriminator;
		public string avatar;
	}

	public enum Reply
	{
		No = 0,
		Yes = 1,
		Ignore = 2
	}
	
	public static void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId)
	{
		Callbacks = handlers;

		EventHandlers staticEventHandlers = new EventHandlers();
		staticEventHandlers.readyCallback += DiscordRpc.ReadyCallback;
		staticEventHandlers.disconnectedCallback += DiscordRpc.DisconnectedCallback;
		staticEventHandlers.errorCallback += DiscordRpc.ErrorCallback;
		staticEventHandlers.joinCallback += DiscordRpc.JoinCallback;
		staticEventHandlers.spectateCallback += DiscordRpc.SpectateCallback;
		staticEventHandlers.requestCallback += DiscordRpc.RequestCallback;

		InitializeInternal(applicationId, ref staticEventHandlers, autoRegister, optionalSteamId);
	}
	
	[DllImport(path, EntryPoint = "Discord_Initialize", CallingConvention = CallingConvention.Cdecl)]
	static extern void InitializeInternal(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId);

	[DllImport(path, EntryPoint = "Discord_Shutdown", CallingConvention = CallingConvention.Cdecl)]
	public static extern void Shutdown();

	[DllImport(path, EntryPoint = "Discord_RunCallbacks", CallingConvention = CallingConvention.Cdecl)]
	public static extern void RunCallbacks();

	[DllImport(path, EntryPoint = "Discord_UpdatePresence", CallingConvention = CallingConvention.Cdecl)]
	static extern void UpdatePresenceNative(ref RichPresenceStruct presenceStruct);

	[DllImport(path, EntryPoint = "Discord_ClearPresence", CallingConvention = CallingConvention.Cdecl)]
	public static extern void ClearPresence();

	[DllImport(path, EntryPoint = "Discord_Respond", CallingConvention = CallingConvention.Cdecl)]
	public static extern void Respond(string userId, Reply reply);

	[DllImport(path, EntryPoint = "Discord_UpdateHandlers", CallingConvention = CallingConvention.Cdecl)]
	public static extern void UpdateHandlers(ref EventHandlers handlers);

	public static void UpdatePresence(RichPresence presence)
	{
		var presencestruct = presence.GetStruct();
		UpdatePresenceNative(ref presencestruct);
		presence.FreeMem();
	}

	public class RichPresence
	{
		private RichPresenceStruct _presence;
		private readonly List<IntPtr> _buffers = new List<IntPtr>(10);

		public string state; /* max 128 bytes */
		public string details; /* max 128 bytes */
		public long startTimestamp;
		public long endTimestamp;
		public string largeImageKey; /* max 32 bytes */
		public string largeImageText; /* max 128 bytes */
		public string smallImageKey; /* max 32 bytes */
		public string smallImageText; /* max 128 bytes */
		public string partyId; /* max 128 bytes */
		public int partySize;
		public int partyMax;
		public string matchSecret; /* max 128 bytes */
		public string joinSecret; /* max 128 bytes */
		public string spectateSecret; /* max 128 bytes */
		public bool instance;

		/// <summary>
		/// Get the <see cref="RichPresenceStruct"/> reprensentation of this instance
		/// </summary>
		/// <returns><see cref="RichPresenceStruct"/> reprensentation of this instance</returns>
		internal RichPresenceStruct GetStruct()
		{
			if (_buffers.Count > 0)
			{
				FreeMem();
			}

			_presence.state = StrToPtr(state);
			_presence.details = StrToPtr(details);
			_presence.startTimestamp = startTimestamp;
			_presence.endTimestamp = endTimestamp;
			_presence.largeImageKey = StrToPtr(largeImageKey);
			_presence.largeImageText = StrToPtr(largeImageText);
			_presence.smallImageKey = StrToPtr(smallImageKey);
			_presence.smallImageText = StrToPtr(smallImageText);
			_presence.partyId = StrToPtr(partyId);
			_presence.partySize = partySize;
			_presence.partyMax = partyMax;
			_presence.matchSecret = StrToPtr(matchSecret);
			_presence.joinSecret = StrToPtr(joinSecret);
			_presence.spectateSecret = StrToPtr(spectateSecret);
			_presence.instance = instance;

			return _presence;
		}

		/// <summary>
		/// Returns a pointer to a representation of the given string with a size of maxbytes
		/// </summary>
		/// <param name="input">String to convert</param>
		/// <returns>Pointer to the UTF-8 representation of <see cref="input"/></returns>
		private IntPtr StrToPtr(string input)
		{
			if (string.IsNullOrEmpty(input)) return IntPtr.Zero;
			var convbytecnt = Encoding.UTF8.GetByteCount(input);
			var buffer = Marshal.AllocHGlobal(convbytecnt + 1);
			for (int i = 0; i < convbytecnt + 1; i++)
			{
				Marshal.WriteByte(buffer, i, 0);
			}
			_buffers.Add(buffer);
			Marshal.Copy(Encoding.UTF8.GetBytes(input), 0, buffer, convbytecnt);
			return buffer;
		}

		/// <summary>
		/// Convert string to UTF-8 and add null termination
		/// </summary>
		/// <param name="toconv">string to convert</param>
		/// <returns>UTF-8 representation of <see cref="toconv"/> with added null termination</returns>
		private static string StrToUtf8NullTerm(string toconv)
		{
			var str = toconv.Trim();
			var bytes = Encoding.Default.GetBytes(str);
			if (bytes.Length > 0 && bytes[bytes.Length - 1] != 0)
			{
				str += "\0\0";
			}
			return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(str));
		}

		/// <summary>
		/// Free the allocated memory for conversion to <see cref="RichPresenceStruct"/>
		/// </summary>
		internal void FreeMem()
		{
			for (var i = _buffers.Count - 1; i >= 0; i--)
			{
				Marshal.FreeHGlobal(_buffers[i]);
				_buffers.RemoveAt(i);
			}
		}
	}
}
#endif

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MongoDB.Bson.IO;

public struct HttpResponse
{
	public int StatusCode;
	public int Result;
	public string[] Headers;
	public Dictionary Body;
}

public partial class HttpController : Node
{
	private readonly string[] defaultHeaders = new string[]
	{
			"Content-Type: application/json",
	};

	private string[] AppendHeaders(string[] headers, int bodyLength) {
		var newHeaders = new string[headers.Length + defaultHeaders.Length + 1];
		for (int i = 0; i < headers.Length; i++)
		{
			newHeaders[i] = headers[i];
		}
		for (int i = 0; i < defaultHeaders.Length; i++)
		{
			newHeaders[i + headers.Length] = defaultHeaders[i];
		}
		newHeaders[^1] = "Content-Length: " + bodyLength;
		return newHeaders;
	}

	public async Task<HttpResponse> Post(string url, string[] headers, string body)
	{
		headers = AppendHeaders(headers, body.Length);
		var requestNode = new HttpRequest();
		AddChild(requestNode);
		requestNode.Request(url, headers, HttpClient.Method.Post, body);
		var response = await ToSignal(requestNode, "request_completed");
		requestNode.QueueFree();
		return new HttpResponse() {
			Result = (int)response[0],
			StatusCode = (int)response[1],
			Headers = (string[])response[2],
			Body = (Dictionary)Json.ParseString(Encoding.UTF8.GetString((byte[])response[3]))
		};
	}

	public async Task<HttpResponse> Post(string url, string body)
	{
		return await Post(url, System.Array.Empty<string>(), body);
	}

	public async Task<HttpResponse> Post(string url, string[] headers, Dictionary body)
	{
		return await Post(url, headers, Json.Stringify(body));
	}

	public async Task<HttpResponse> Post(string url, Dictionary body)
	{
		return await Post(url, System.Array.Empty<string>(), Json.Stringify(body));
	}
}

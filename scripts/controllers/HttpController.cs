using System.Text;
using System.Threading.Tasks;
using Godot;

public struct HttpResponse
{
    public int StatusCode;
    public int Result;
    public string[] Headers;
    public Godot.Collections.Dictionary Body;
}

public class HttpController : Node
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
        newHeaders[newHeaders.Length - 1] = "Content-Length: " + bodyLength;
        return newHeaders;
    }

    public async Task<HttpResponse> Post(string url, string[] headers, string body)
    {
        headers = AppendHeaders(headers, body.Length);
        var requestNode = new HTTPRequest();
        AddChild(requestNode);
        requestNode.Request(url, headers, true, HTTPClient.Method.Post, body);
        var response = await ToSignal(requestNode, "request_completed");
        requestNode.QueueFree();
        return new HttpResponse() {
            Result = (int)response[0],
            StatusCode = (int)response[1],
            Headers = (string[])response[2],
            Body = JSON.Parse(Encoding.UTF8.GetString((byte[])response[3])).Result as Godot.Collections.Dictionary
        };
    }

    public async Task<HttpResponse> Post(string url, string body)
    {
        return await Post(url, new string[] {}, body);
    }

    public async Task<HttpResponse> Post(string url, string[] headers, Godot.Collections.Dictionary body)
    {
        return await Post(url, headers, JSON.Print(body));
    }

    public async Task<HttpResponse> Post(string url, Godot.Collections.Dictionary body)
    {
        return await Post(url, new string[] {}, JSON.Print(body));
    }
}
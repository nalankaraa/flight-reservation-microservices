using System.Net.Http;

namespace Dispatcher.Application.Forwarding;

public interface IRequestForwarder
{
	Task<HttpResponseMessage> ForwardAsync(
		string method,
		string targetUrl,
		Dictionary<string, string> headers,
		Stream body
	);
}
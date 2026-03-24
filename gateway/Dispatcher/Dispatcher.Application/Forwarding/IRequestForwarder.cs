namespace Dispatcher.Application.Forwarding;

public interface IRequestForwarder
{
	Task<string> ForwardAsync(string targetUrl);
}
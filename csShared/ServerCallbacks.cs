using System.Net.Sockets;

namespace SocketServer
{
    /// <summary>
    /// Delegates that allow shared library code to interact with server-specific functionality
    /// (e.g. sending messages, looking up users/locations).
    /// The server project should assign these delegates during startup.
    /// </summary>
    public static class ServerCallbacks
    {
        public static Action<TcpClient, string>? SendMessage { get; set; }
        public static Func<string, User>? FindOnlineUser { get; set; }
        public static Func<string, Location>? FindLocation { get; set; }
    }
}
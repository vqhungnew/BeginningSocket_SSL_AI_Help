using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;

class Program
{
    static void Main(string[] args)
    {
        // Call the method to send a secure request.
        SendHttpRequest(new Uri("https://www.google.com"));
    }

    private static void SendHttpRequest(Uri? uri = null, int port = 443)
    {
        uri ??= new Uri("https://google.com");

        // Construct a minimalistic HTTP/1.1 request
        byte[] requestBytes = Encoding.ASCII.GetBytes(@$"GET {uri.PathAndQuery} HTTP/1.1
Host: {uri.Host}
Connection: Close

");

        // Create a TCP/IP socket and connect it to the server
        using Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(uri.Host, port);

        // Create an SSL stream and authenticate as the client
        using SslStream sslStream = new SslStream(new NetworkStream(socket), false);
        sslStream.AuthenticateAsClient(uri.Host, null, SslProtocols.Tls12 | SslProtocols.Tls13, checkCertificateRevocation: true);

        // Send the request using the SSL stream
        sslStream.Write(requestBytes);
        sslStream.Flush();

        // Do minimalistic buffering assuming ASCII response
        byte[] responseBytes = new byte[256];
        char[] responseChars = new char[256];

        while (true)
        {
            int bytesReceived = sslStream.Read(responseBytes, 0, responseBytes.Length);

            // Receiving 0 bytes means EOF has been reached
            if (bytesReceived == 0) break;

            // Convert byteCount bytes to ASCII characters using the 'responseChars' buffer as destination
            int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);

            // Print the contents of the 'responseChars' buffer to Console.Out
            Console.Out.Write(responseChars, 0, charCount);
            //Waiting any action to exit
            Console.ReadKey();
        }
    }
}


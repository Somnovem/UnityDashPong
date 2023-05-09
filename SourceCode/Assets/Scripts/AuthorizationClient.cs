using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class AuthorizationClient 
{
    public delegate void ConnectionResultDelegate(AuthorizationResult result);
    public event ConnectionResultDelegate ConnectionResult;

    public static string serverIP;  

    public void Login(string username,string password)
    {   
        SendAuthorizationCommand("Login",username,password);
    }

    public void Signup(string username,string password)
    {
        SendAuthorizationCommand("Signup",username,password);
    }

    public static void Logout(string username)
    {
        TcpClient client = new TcpClient();
        client.Connect(serverIP,8001);
        NetworkStream stream = client.GetStream();
        byte[] data = Encoding.UTF8.GetBytes($"Exit:{username}");
        stream.Write(data, 0, data.Length);
        client.Close();
    }

    private TcpClient _client;
    private NetworkStream _stream;
    private readonly byte[] _buffer = new byte[1024];

    private void SendAuthorizationCommand(string command,string username,string password)
    {
        Task.Run(async ()=>
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync("192.168.0.109", 8001); 
                _stream = _client.GetStream();
                var credentialMessage = $"{command}:{username} {PasswordHasher.ComputeSHA256Hash(password)}";
                var bytesToSend = Encoding.UTF8.GetBytes(credentialMessage);
                await _stream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                var bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length);
                var response = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
                int loginSuccessful = int.Parse(response);
                ConnectionResult?.Invoke((AuthorizationResult)loginSuccessful);
            }
            catch
            {
                ConnectionResult?.Invoke(AuthorizationResult.ServerClosed);
            }
        });

    }
}

public enum AuthorizationResult
{
    Success,
    InvalidCredentials,
    AlreadyOnline,
    Banned,
    ServerClosed
}
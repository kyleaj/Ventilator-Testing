using System;
using System.Collections.Generic;
using System.Text;

namespace VentilatorTestConsole
{
    public class CommunicationService
    {
    }


public class LocalCommunicationService
{

    private readonly WebServer ventilatorServer;
    private ClientWebSocket ClientLink;
    private readonly CancellationToken StopEverything;

    public LocalCommunicationService(string ip)
    {
        // For now, just use the first IP. In the future, maybe test each address? Or look for IPv4
        ClientLink = new ClientWebSocket();
        StopEverything = new CancellationToken();
        Debug.WriteLine($"Trying to connect to ws://{ip}:54321/");
        ClientLink
            .ConnectAsync(new Uri($"ws://{ip}:54321/"), StopEverything)
            .ContinueWith((res) =>
            {
                if (res.IsFaulted)
                {
                    Debug.WriteLine("Could not connect :(");
                    Debug.WriteLine(res.Exception);
                }
                else if (res.IsCanceled)
                {
                    Debug.WriteLine("Connecting cancelled");
                }
                else
                {
                    Debug.WriteLine("Successfully connected!");
                    StartReadingMessages();
                }
            });
    }

    private void GetCsv() {
        WebSocketReceiveResult result;
        
    }

    private void StartReadingMessages()
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                WebSocketReceiveResult result;
                var message = new ArraySegment<byte>(new byte[4096]);
                StringBuilder serializedMessage = new StringBuilder();
                do
                {
                    result = await ClientLink.ReceiveAsync(message, StopEverything);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
                        string thisMessage = Encoding.UTF8.GetString(messageBytes);
                        serializedMessage.Append(thisMessage);
                        if (!result.EndOfMessage)
                        {
                            Debug.WriteLine("Received partial message: " + thisMessage);
                        }
                    }
                } while (!result.EndOfMessage);
                // TODO: Proccess message receipt
                string fullMessage = serializedMessage.ToString();
                Debug.WriteLine("Received message: " + fullMessage);
                var mess = JsonConvert.DeserializeObject<Message>(fullMessage);
                switch (mess.Type)
                {
                    case MessageType.Who:
                        await HandleWho();
                        break;
                    case MessageType.Rules:
                        HandleRules(mess);
                        break;
                    case MessageType.CurrentPlayers:
                        HandleCurrentPlayersReceived(mess);
                        break;
                    case MessageType.PlayerAdded:
                    case MessageType.PlayerChanged:
                    case MessageType.PlayerRemoved:
                        HandlePlayersDeltaUpdate(mess);
                        break;
                    case MessageType.Start:
                        HandleStartGame();
                        break;
                }
            }
        }, StopEverything, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

/*
    private async Task HandleWho()
    {
        var response = new Message
        {
            Type = MessageType.Im,
            Data = JsonConvert
                .SerializeObject(
                    (Application.Current as App).CurrentGame.State.Me)
        };
        await GameLink.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response.ToString())),
            WebSocketMessageType.Text, true, StopEverything);
    }

    public void HandleStartGame()
    {

    }

    private void HandleRules(Message message)
    {
        (Application.Current as App).CurrentGame.Settings =
            JsonConvert.DeserializeObject<GameParameters>(message.Data);
        Debug.WriteLine("Got rules!");
    }

    private void HandleCurrentPlayersReceived(Message message)
    {
        var players = JsonConvert.DeserializeObject<List<PlayerInfo>>(message.Data);
        Debug.WriteLine("Got players!");
#if WINDOWS_UWP
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
#else
        Device.BeginInvokeOnMainThread(() =>
        {
#endif
            foreach (var player in players)
            {
                Debug.WriteLine("Adding: " + player.Name);
                (Application.Current as App).CurrentGame.State.Players.Add(player);
            }
        })

#if WINDOWS_UWP
            .AsTask().Wait()
#endif
            ;
        Debug.WriteLine("Added all players!");
    }

    private void HandlePlayersDeltaUpdate(Message message)
    {
#if WINDOWS_UWP
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
#else
        Device.BeginInvokeOnMainThread(() =>
        {
#endif
            switch (message.Type)
            {
                case MessageType.PlayerAdded:
                    (Application.Current as App).CurrentGame.State.Players
                        .Add(JsonConvert.DeserializeObject<PlayerInfo>(message.Data));
                    break;
                case MessageType.PlayerChanged:
                    Debug.WriteLine("Players can't change yet!");
                    break;
                case MessageType.PlayerRemoved:
                    (Application.Current as App).CurrentGame.State.Players
                        .Remove(JsonConvert.DeserializeObject<PlayerInfo>(message.Data));
                    break;
            }
        })
#if WINDOWS_UWP
            .AsTask().Wait()
#endif
            ;
        }*/
    }
}

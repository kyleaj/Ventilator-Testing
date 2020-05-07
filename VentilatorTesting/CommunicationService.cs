using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;
using Unosquare.Swan;
using Windows.UI.Xaml;

namespace VentilatorTesting
{
    public class CommunicationService
    {
        WebServer server;
        VentilatorConnectionsHandler handler;

        public CommunicationService()
        {
            string sHostName = Dns.GetHostName();
            IPAddress[] IPs = Dns.GetHostAddressesAsync(sHostName).GetAwaiter().GetResult();
            // TODO: Make this flexible
            IPs = Array.FindAll(IPs, a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            Debug.WriteLine("Starting server on IP " + IPs[0].ToString());

            server = new WebServer($"http://{IPs[0].ToString()}:54321/", RoutingStrategy.Regex);

            server.RegisterModule(new WebSocketsModule());

            server.RegisterModule(new FallbackModule(async (ctx, ct) =>
            {
                await ctx.JsonResponseAsync(new { Hola = "404: Not found" });
                return true;
            }));

            handler = new VentilatorConnectionsHandler();

            server.Module<WebSocketsModule>().RegisterWebSocketsServer<VentilatorConnectionsHandler>("/TestVent", handler);

            server.StateChanged += Server_StateChanged;

        }

        private void Server_StateChanged(object sender, Unosquare.Labs.EmbedIO.Core.WebServerStateChangedEventArgs e)
        {
            Debug.WriteLine($"Server state change from {e.OldState} to {e.NewState}");
        }

        public void SendVolumeUpdate(float update, Enums.Patient patient)
        {
            handler.SendMessage(new Message
            {
                Type = Message.MessageType.VolumeUpdate,
                Data = update,
                AffectedPatient = patient
            });
        }

        public void SendPressureUpdate(bool update, Enums.Patient patient)
        {
            handler.SendMessage(new Message
            {
                Type = Message.MessageType.PressureUpdate,
                Data = update,
                AffectedPatient = patient
            });
        }

        public void SendTestStatusUpdate(Test.TestStatus update)
        {
            handler.SendMessage(new Message
            {
                Type = Message.MessageType.VolumeUpdate,
                Data = update,
                AffectedPatient = Enums.Patient.Both
            });
        }
    }

    class VentilatorConnectionsHandler : WebSocketsServer
    {
        public override string ServerName => throw new NotImplementedException();

        protected override void OnClientConnected(IWebSocketContext context, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            Debug.WriteLine("New client connected");
        }

        protected override void OnClientDisconnected(IWebSocketContext context)
        {
            Debug.WriteLine("Client disconnected");
        }

        protected override void OnFrameReceived(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
        {
            // Whatever
        }

        protected override void OnMessageReceived(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
        {
            string message_raw = buffer.ToText();
            Message message = JsonConvert.DeserializeObject<Message>(message_raw);

            switch (message.Type)
            {
                case Message.MessageType.StartTestRequest:
                    var data = (Tuple<string, int>)message.Data;
                    (Application.Current as App).Sensors.StartTest(data.Item1, data.Item2); // TODO: Handle test already running
                    break;
                case Message.MessageType.StopTestRequest:
                    (Application.Current as App).Sensors.StopTest();
                    break;
                case Message.MessageType.TestIndexRequest:
                    break;
                case Message.MessageType.TestResultRequest:
                    break;
                
            }
        }

        private void HandleTestIndexRequest()
        {
            // Send a list of the filenames in the Application's local folder
        }

        private void HandleTestResultRequest()
        {
            // Can we send this (possibly big) file over web sockets?
        }

        public void SendMessage(Message message)
        {
            string mess = JsonConvert.SerializeObject(message);
            foreach (var socket in this.WebSockets)
            {
                Send(socket, mess);
            }
        }
    }
}

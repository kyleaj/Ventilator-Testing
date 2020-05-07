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
using Windows.Storage;
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

#pragma warning disable CS0618 // Type or member is obsolete
            server.RegisterModule(new FallbackModule(async (ctx, ct) =>
            {
                await ctx.JsonResponseAsync(new { Hola = "404: Not found" });
                return true;
            }));
#pragma warning restore CS0618 // Type or member is obsolete



            handler = new VentilatorConnectionsHandler();

            server.Module<WebSocketsModule>().RegisterWebSocketsServer<VentilatorConnectionsHandler>("/TestVent", handler);

            server.RegisterModule(new WebApiModule());
            server.Module<WebApiModule>().RegisterController<PostController>();

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
        public override string ServerName => "VentTestServ";

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
                    HandleTestIndexRequest(context);
                    break;
                case Message.MessageType.TestResultRequest:
                    break;
                
            }
        }

        private void HandleTestIndexRequest(IWebSocketContext requester)
        {
            // Send a list of the filenames in the Application's local folder
            ApplicationData.Current.LocalFolder.GetFilesAsync().AsTask().ContinueWith((res) =>
            {
                string response;
                if (res.IsFaulted)
                {
                    response = JsonConvert.SerializeObject(
                        new Message {
                            Type = Message.MessageType.TestIndexResponse,
                            Data = null
                        });
                } else
                {
                    IReadOnlyList<StorageFile> result = res.Result;
                    IEnumerable<string> testNames = result.Select((a) => a.DisplayName);
                    response = JsonConvert.SerializeObject(
                        new Message
                        {
                            Type = Message.MessageType.TestIndexResponse,
                            Data = testNames
                        });
                }
                Send(requester, response);
            });
        }

        /**private void HandleTestResultRequest(string providerPath)
        {
            // Can we send this (possibly big) file over web sockets?
            // I still have no idea what I'm doing
            FileStream stream = IFileProvider.OpenFile(providerPath);
            string contents;
            using (StreamReader reader = new StreamReader(stream))
            {
                contents = reader.ReadToEnd();
            }
            Message message = JsonConvert.DeserializeObject<Message>(contents);
            SendMessage(message);
        }**/

        public void SendMessage(Message message)
        {
            string mess = JsonConvert.SerializeObject(message);
            foreach (var socket in this.WebSockets)
            {
                Send(socket, mess);
            }
        }
    }

    public class PostController : WebApiController
    {
        public PostController(IHttpContext context) : base(context)
        {
        }

        [WebApiHandler(HttpVerbs.Get, "/api/pressuredata/{filename}")]
        public async Task<bool> GetFileName(string filename)
        {
            try
            {
                return await Ok(await ApplicationData.Current.LocalFolder.GetFileAsync(filename));
            }
            catch (Exception ex)
            {
                return await InternalServerError(ex);
            }
        }
    }
}

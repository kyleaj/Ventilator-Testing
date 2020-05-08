using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Tmds.MDns;
using Xamarin.Forms;

namespace VentilatorTestConsole
{
    public class CommunicationService
    {
        private ClientWebSocket VentilatorLink;
        public ObservableCollection<Ventilator> FoundVentilators;
        private bool StayConnected;
        private ServiceBrowser SB;

        public CommunicationService()
        {
            // Start searching for Ventilators
            FoundVentilators = new ObservableCollection<Ventilator>();
            StayConnected = false;

            Debug.WriteLine("Creating new service browser");
            SB = new ServiceBrowser();
            SB.ServiceAdded += OnServiceAdded;
            SB.ServiceRemoved += OnServiceRemoved;
            string serviceType = "_venttest._tcp";
            Console.WriteLine("Browsing for type: {0}", serviceType);
            SB.StartBrowse(serviceType);
        }

        public async Task StartTest()
        {
            Message mess = new Message()
            {
                Type = Message.MessageType.StartTestRequest,
                Data = new Tuple<string, int>("Test", 60 * 60) // Seconds in an hour
            };

            string serialized_message = JsonConvert.SerializeObject(mess);

            await VentilatorLink.SendAsync(new ArraySegment<byte> (Encoding.UTF8.GetBytes(serialized_message.ToString())), 
                WebSocketMessageType.Text, true, new System.Threading.CancellationToken());
        }

        public async Task StopTest()
        {
            Message mess = new Message()
            {
                Type = Message.MessageType.StopTestRequest,
                Data = null
            };

            string serialized_message = JsonConvert.SerializeObject(mess);

            await VentilatorLink.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(serialized_message.ToString())),
                WebSocketMessageType.Text, true, new System.Threading.CancellationToken());
        }

        private void OnServiceRemoved(object sender, ServiceAnnouncementEventArgs e)
        {
            IPAddress id = e.Announcement.Addresses.First();
            Ventilator toRemove = null;
            foreach (var disc in FoundVentilators)
            {
                if (IPAddress.Parse(disc.IP) == id)
                {
                    toRemove = disc;
                    break;
                }
            }
            if (toRemove != null)
            {
                FoundVentilators.Remove(toRemove);
            }
        }

        private void OnServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            var detectedVent = new Ventilator
            {
                IP = e.Announcement.Addresses.First().ToString(),
            };

            foreach (var txt in e.Announcement.Txt)
            {
                var split = txt.Split('=');
                var id = split[0];
                if (id == "fn")
                {
                    detectedVent.Name = split[1];
                }
            }

            bool found = false;
            foreach (var disc in FoundVentilators)
            {
                if (disc.IP == detectedVent.IP)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                FoundVentilators.Add(detectedVent);
            }
        }

        public async Task ConnectToVentilator(IPAddress ip)
        {
            if (VentilatorLink != null)
            {
                StayConnected = false;
                await Task.Delay(1000); // Finish anything we're doing
                await VentilatorLink.CloseAsync(WebSocketCloseStatus.Empty, "Bye", new System.Threading.CancellationToken());
                VentilatorLink.Dispose();
            }
            StayConnected = true;
            VentilatorLink = new ClientWebSocket();
            Debug.WriteLine($"Trying to connect to ws://{ip}:54321/TestVent");
            await VentilatorLink.ConnectAsync(new Uri($"ws://{ip}:54321/TestVent"), new System.Threading.CancellationToken());
            Debug.WriteLine("Success connecting!");
            ReadMessages();
        }

        private void ReadMessages()
        {
            Task.Factory.StartNew(async () =>
            {
                while (StayConnected)
                {
                    WebSocketReceiveResult result;
                    var message = new ArraySegment<byte>(new byte[4096]);
                    StringBuilder serializedMessage = new StringBuilder();
                    do
                    {
                        result = await VentilatorLink.ReceiveAsync(message, new System.Threading.CancellationToken());
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
                    var mess = JsonConvert.DeserializeObject<Message>(fullMessage);
                    switch (mess.Type)
                    {
                        case Message.MessageType.PressureUpdate:
                            double dp = (double)mess.Data;
                            if (mess.AffectedPatient == Patient.A)
                                (Application.Current as App).StatService.Patient1.RecentPressMeasurements.Add((float)dp);
                            else
                                (Application.Current as App).StatService.Patient2.RecentPressMeasurements.Add((float)dp);
                            break;
                        case Message.MessageType.TestIndexResponse:
                            break;
                        case Message.MessageType.TestStatusUpdate:
                            (Application.Current as App).StatService.CurrTest = (PatientStatusService.TestStatus)mess.Data;
                            break;
                        case Message.MessageType.VolumeUpdate:
                            double d = (double)mess.Data;
                            if (mess.AffectedPatient == Patient.A)
                                (Application.Current as App).StatService.Patient1.RecentVolumMeasurements.Add((float)d);
                            else
                                (Application.Current as App).StatService.Patient2.RecentVolumMeasurements.Add((float)d);
                            break;
                    }
                }
            }, new System.Threading.CancellationToken(), TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

    }
}
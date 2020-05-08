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
using Xamarin.Forms;

namespace VentilatorTestConsole
{
    public class CommunicationService
    {
        private ClientWebSocket VentilatorLink;
        public ObservableCollection<Ventilator> FoundVentilators;
        private bool StayConnected;

        public CommunicationService()
        {
            // Start searching for Ventilators
            FoundVentilators = new ObservableCollection<Ventilator>();
            StayConnected = false;
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
            Debug.WriteLine($"Trying to connect to ws://{ip}:54321/");
            await VentilatorLink.ConnectAsync(new Uri($"ws://{ip}:54321/"), new System.Threading.CancellationToken());
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
                    Debug.WriteLine("Received message: " + fullMessage);
                    var mess = JsonConvert.DeserializeObject<Message>(fullMessage);
                    switch (mess.Type)
                    {
                        case Message.MessageType.PressureUpdate:
                            if (mess.AffectedPatient == Patient.A)
                                (Application.Current as App).StatService.Patient1.RecentPressMeasurements.Add((float)mess.Data);
                            else
                                (Application.Current as App).StatService.Patient2.RecentPressMeasurements.Add((float)mess.Data);
                            break;
                        case Message.MessageType.TestIndexResponse:
                            break;
                        case Message.MessageType.TestStatusUpdate:
                            (Application.Current as App).StatService.CurrTest = (PatientStatusService.TestStatus)mess.Data;
                            break;
                        case Message.MessageType.VolumeUpdate:
                            if (mess.AffectedPatient == Patient.A)
                                (Application.Current as App).StatService.Patient1.RecentVolumMeasurements.Add((float)mess.Data);
                            else
                                (Application.Current as App).StatService.Patient2.RecentVolumMeasurements.Add((float)mess.Data);
                            break;
                    }
                }
            }, new System.Threading.CancellationToken(), TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

    }
}
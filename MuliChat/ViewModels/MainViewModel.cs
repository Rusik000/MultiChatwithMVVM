using MuliChat.Command;
using SimpleTcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MuliChat.ViewModels
{
    public class MainViewModel : BaseViewModel
    {

        TcpClient client;
        TcpListener listener;

        public StreamReader STR;
        public StreamWriter STW;

        public string Receive;
        public string TextToSend;

        private readonly BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private readonly BackgroundWorker backgroundWorker2 = new BackgroundWorker();

        public MainWindow MainView { get; set; }

        public RelayCommand StartCommand { get; set; }

        public RelayCommand ConnectCommand { get; set; }

        public RelayCommand SendCommand { get; set; }

        public MainViewModel()
        {
            backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker2.DoWork += BackgroundWorker2_DoWork;

            StartCommand = new RelayCommand((sender) =>
            {
                Thread serverThread = new Thread(() =>
                {
                    Start();
                });
                serverThread.Start();
            });

            ConnectCommand = new RelayCommand((sender) =>
            {
                Thread connectThread = new Thread(() =>
                {
                    Connect();
                });
                connectThread.Start();
            });
            SendCommand = new RelayCommand((sender) =>
            {
                Send();
            });
        }
        public void Start()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var ipadress = IPAddress.Parse(MainView.ServerIpTextbox.Text);
                var port = int.Parse(MainView.ServerPortTextBox.Text);
                listener = new TcpListener(ipadress, port);
            });

            listener.Start();
            MessageBox.Show("Server is listening . . .");
            client = listener.AcceptTcpClient();

            var thread = new Thread(() =>
            {
                STR = new StreamReader(client.GetStream());
                STW = new StreamWriter(client.GetStream());
                STW.AutoFlush = true;
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.WorkerSupportsCancellation = true;

            });

            thread.Start();

        }
        public void Connect()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var ipadress = IPAddress.Parse(MainView.ClientIpTextbox.Text);
                var port = int.Parse(MainView.ClientPortTextbox.Text);
                IPEndPoint ep = new IPEndPoint(ipadress, port);
                client = new TcpClient(MainView.ClientIpTextbox.Text, port);
            });

            try
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    MainView.ChatScreenTextbox.Text += "Connect To Server\n";
                    STW = new StreamWriter(client.GetStream());
                    STR = new StreamReader(client.GetStream());
                });
                STW.AutoFlush = true;
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.WorkerSupportsCancellation = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Send()
        {
            if (MainView.MessageTextBox.Text != "")
            {
                TextToSend = MainView.MessageTextBox.Text;
                backgroundWorker2.RunWorkerAsync();
            }
            MainView.MessageTextBox.Text = " ";
        }
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                try
                {
                    Receive = STR.ReadLine();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        MainView.ChatScreenTextbox.Text += "You : " + Receive + "\n";
                        Receive = "";
                    });
                }
                catch (Exception)
                {

                }
            }
        }
        private void BackgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (client.Connected)
            {
                STW.WriteLine(TextToSend);
                App.Current.Dispatcher.Invoke(() =>
                {
                    MainView.ChatScreenTextbox.Text += "Me : " + TextToSend + "\n";

                });
            }
            else
            {
                MessageBox.Show("Sending Failed");
            }
            backgroundWorker2.CancelAsync();
        }
    }
}

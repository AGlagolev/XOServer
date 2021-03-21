using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XOServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _serverStart = false;
        private TcpListener _list;
        private User _user1;
        private User _user2;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_ServerStart_Click(object sender, RoutedEventArgs e)
        {
            if (_list != null)
            {
                label_Status.Content = "Server already started";
                return;
            }
            string IP = textBox_IP.Text;
            string PORT = textBox_Port.Text;

            IPAddress address;

            try
            {
                if (IPAddress.TryParse(IP, out address) && PORT.Length > 0)
                {
                    _list = new TcpListener(address, Convert.ToInt32(PORT));
                    _list.Start();
                    label_Status.Content = "Server started";
                    _serverStart = true;
                    Thread newThread = new Thread(new ThreadStart(AnotherThreadReadMessages));
                    newThread.IsBackground = true;
                    newThread.Start();

                }
                else
                {
                    label_Status.Content = "Заполните IP address и PORT";
                }
            }
            catch (SocketException socketEx)
            {
                MessageBox.Show("SocketException SERVER:" + socketEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception SERVER:" + ex.Message);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_list != null)
            {
                _list.Stop();
            }
        }

        private void button_ServerStop_Click(object sender, RoutedEventArgs e)
        {
            label_Status.Content = "Server stoped";
            _serverStart = false;
            
            _list.Stop();
            _list = null;
        }

        private void AnotherThreadReadMessages()
        {
            try
            {
                while (_serverStart)
                {
                    TcpClient tcpClient = _list.AcceptTcpClient();
                    EndPoint rep =  tcpClient.Client.RemoteEndPoint;
                    //StreamReader sr = new StreamReader(tcpClient.GetStream(), Encoding.Unicode);
                    /*сообщение от пользователя*/
                    //string userMessage = sr.ReadLine();
                    NetworkStream stream = tcpClient.GetStream();
                    Byte[] bytes = new Byte[256];
                    String data = null;
                    //byte[] byteArr = Encoding.Unicode.GetBytes("SERVER");
                    //networkStream.Write(byteArr, 0, byteArr.Length);
                    
                    int i;

                    // Принимаем данные от клиента в цикле пока не дойдём до конца.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Преобразуем данные в ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        // Преобразуем строку к верхнему регистру.
                        //data = data.ToUpper();
                        string answer = MessageSwitch(data, ParseEPoint(rep));
                        // Преобразуем полученную строку в массив Байт.
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(answer);

                        // Отправляем данные обратно клиенту (ответ).
                        stream.Write(msg, 0, msg.Length);

                    }










                    //MessageSwitch(userMessage, ParseEPoint(rep), tcpClient);



                    /*добавляем клиентское сообщение в наш листбокс*/
                    //if (userMessage == "shutdown")
                    //{
                    //    //this.Dispatcher.Invoke(() =>
                    //    //{
                    //    //    wind.Close();
                    //    //}); // робота с элементом основного потока
                    //}
                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    listBox_Messages.Items.Add(userMessage);
                    //}); // робота с элементом основного потока

                    tcpClient.Close(); // Закрываем подключение к клиенту
                }
            }
            catch (Exception)
            {

            }

        }

        private string ParseEPoint(EndPoint ep)
        {
            string epstr = ep.ToString();
            int pos = epstr.IndexOf(':');
            return epstr.Substring(0,pos);
        }

        private string ParseMessage(string str, string mode = "getFlag")
        {
            int pos;
            switch (mode)
            {
                case "getFlag":
                    pos = str.IndexOf(':');
                    return str.Substring(0, pos);
                case "getText":
                    pos = str.IndexOf(':') + 1;
                    return str.Substring(pos);
            }
            return null;
        }

        private string MessageSwitch(string mes, string ip)
        {
            string res = null;
            string flag = ParseMessage(mes);
            string mes_text = ParseMessage(mes, "getText");
            switch (flag)
            {
                case "NewUser":
                    if (_user1 != null && _user2 != null)
                    {
                        MessageBox.Show("Sorry all users in game");
                        res = "error";
                        break;
                    }
                    if (_user1 != null && _user2 == null && mes_text != _user1.Name)
                    {
                        _user2 = new User(ip, mes_text);
                        this.Dispatcher.Invoke(() =>
                        {
                            label_User2Status.Content = $"{mes_text}@{ip} in game";
                            res = "O";
                        }); // робота с элементом основного потока

                    }
                    if (_user1 == null)
                    {
                        _user1 = new User(ip, mes_text);
                        this.Dispatcher.Invoke(() =>
                        {
                            label_User1Status.Content = $"{mes_text}@{ip} in game";
                            res = "X";
                        }); // робота с элементом основного потока

                    }
                    break;
            }
            return res;
        }
    }
}

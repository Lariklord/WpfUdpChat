using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfUdpChat
{
    internal class ChatViewModel:INotifyPropertyChanged
    {
        const string HOST = "235.5.5.1";
        private UdpClient client;

        private string localPort;
        public string LocalPort
        {
            get { return localPort; }
            set
            {
                localPort = value;
                CanEnter=EnterValidate();
                OnPropertyChanged("LocalPort");
            }
        }
        
        private string remotePort;
        public string RemotePort
        {
            get { return remotePort; }
            set
            {
                remotePort = value;
                CanEnter = EnterValidate();
                OnPropertyChanged("RemotePort");
            }
        }
       
        private string name;
        public string Name
        {
            set 
            {
                name = value;
                CanEnter = EnterValidate();
                OnPropertyChanged("Name");
            }
            get 
            {
                return name;
            }
        }
        
        private bool canEnter;
        public bool CanEnter
        {
            set
            {
                canEnter = value;
                OnPropertyChanged("CanEnter");
            }
            get
            {
                return canEnter;
            }
        }
        
        private bool canLeave;
        public bool CanLeave
        {
            set
            {
                canLeave = value;
                OnPropertyChanged("CanLeave");
            }
            get
            {
                return canLeave;
            }
        }
        
        private bool canSend;
        public bool CanSend
        {
            set
            {
                canSend = value;
                OnPropertyChanged("CanSend");
            }
            get
            {
                return canSend;
            }
        }

        private bool readOnly;
        public bool ReadOnly
        {
            get
            {
                return readOnly;
            }
            set
            {
                readOnly = value;
                OnPropertyChanged("ReadOnly");
            }
        }

        private string message;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                CanSend = MessageValidate(Message);
                OnPropertyChanged("Message");
            }
        }

        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }
        public ChatViewModel()
        {
            canEnter = false;
            canLeave = false;
            canSend = false;
            readOnly = false;
           
        }

        private RelayCommand enterCommand;
        public RelayCommand EnterCommand
        {
            set { enterCommand = value; }
            get
            {
                return enterCommand ?? (enterCommand = new RelayCommand(obj =>
                {
                    client = new UdpClient(int.Parse(LocalPort));
                    client.JoinMulticastGroup(IPAddress.Parse(HOST), 50);
                    CanEnter = false;
                    CanLeave = true;
                    CanSend = true;
                    ReadOnly = true;

                    try
                    {
                        Task task = new Task(()=>
                        {
                            try
                            {
                                IPEndPoint point = null;
                                while (true)
                                {
                                    byte[] data = client.Receive(ref point);
                                    string receivedMessage = Encoding.Unicode.GetString(data, 0, data.Length);
                                    if (MessageValidate(receivedMessage))
                                    {
                                        Text += receivedMessage + '\n';
                                    }

                                }
                            }
                            catch (Exception)
                            {

                            }
                        });
                        task.Start();
                        string mes = DateTime.Now.ToShortTimeString() + " " + Name + " Вошел в чат";
                        UdpProtocol.Send(client,HOST,int.Parse(remotePort),mes);
                        Text += mes+"\n";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }));
            }
        }
        private RelayCommand leaveCommand;
        public RelayCommand LeaveCommand
        {
            set { leaveCommand = value; }
            get
            {
                return leaveCommand ?? (leaveCommand = new RelayCommand(obj =>
                {
                    CanEnter = true;
                    CanLeave = false;
                    CanSend = false;
                    ReadOnly = false;

                    try
                    {
                        string mes = DateTime.Now.ToShortTimeString() + " " + Name + " Вышел в чата";
                        UdpProtocol.Send(client, HOST, int.Parse(remotePort), mes);
                        Text = "";
                        UdpProtocol.Exit(client,HOST);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }));
            }
        }

        private RelayCommand sendCommand;
        public RelayCommand SendCommand
        {
            set { sendCommand = value; }
            get
            {
                return sendCommand ?? (sendCommand = new RelayCommand(obj =>
                {
                    try
                    {
                        string mes = DateTime.Now.ToShortTimeString() + " " + Name +": "+Message;
                        UdpProtocol.Send(client, HOST, int.Parse(remotePort), mes);
                        Text += Message+"\n";
                        Message = "";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }));
            }
        }

        private bool EnterValidate()
        {
            return PortValidate(LocalPort) && PortValidate(RemotePort) && NameValidate(Name);
        }
        private bool PortValidate(string port)
        {
            try
            {
                int port_ = int.Parse(port);
                if (port_ > 0 && port_ <= 65535)
                    return true;
            }
            catch (Exception)
            {
                
            }
            return false;

        }
        private bool NameValidate(string name)
        {
            return name != null && name.Length > 0;
        }
        private bool MessageValidate(string message)
        {
            return message != null && message.Length > 0;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string s = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
        }
    }
}

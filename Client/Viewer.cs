using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Viewer : Form
    {

        Socket getScreen = null;
        byte[] datas = new byte[90000 * 22999];


        public Viewer()
        {
            InitializeComponent();
        }

        private void Viewer_Load(object sender, EventArgs e)
        {
            string Host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostByName(Host);
            getScreen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //getScreen.Bind(new IPEndPoint(IPAddress.Parse(ip.AddressList[0].ToString()), null));
            getScreen.Bind(new IPEndPoint(IPAddress.Parse(ip.AddressList[0].ToString()), 8501));
            getScreen.Listen(1);
            getScreen.BeginAccept(new AsyncCallback(ConnectedCallback), null);
            
        }


        void ConnectedCallback(IAsyncResult iar)
        {
            Socket socket = getScreen.EndAccept(iar);
            socket.BeginReceive(datas, 0, datas.Length, SocketFlags.None, new AsyncCallback(dataReceived), socket);
            getScreen.BeginAccept(new AsyncCallback(ConnectedCallback), null);

        }

        void dataReceived(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;
            int length = socket.EndReceive(iar);
            byte[] data = new byte[length];
            Array.Copy(datas, data, data.Length);
            if (length < 50)
            {
                string received = Encoding.UTF8.GetString(data);
                int width = int.Parse(received.Substring(0, received.IndexOf(':')));
                int height = int.Parse(received.Substring(received.IndexOf(':') + 1, received.IndexOf('|') + received.IndexOf(':') + 1));
                this.Size = new Size(width + 16, height + 38);
            
            } else
            {
                MemoryStream ms = new MemoryStream(data);
                Image screen = Bitmap.FromStream(ms);
                pbScreen.BackgroundImage = screen;

            }
        }
    }
}

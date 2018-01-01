using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    #region DM
    [Flags()]
    enum DM : int
    {
        DMDUP_UNKNOWN = 0,
        DMDUP_SIMPLEX = 1,
        DMDUP_VERTICAL = 2,
        DMDUP_HORIZONTAL = 3,
        Orientation = 0x1,
        PaperSize = 0x2,
        PaperLength = 0x4,
        PaperWidth = 0x8,
        Scale = 0x10,
        Position = 0x20,
        NUP = 0x40,
        DisplayOrientation = 0x80,
        Copies = 0x100,
        DefaultSource = 0x200,
        PrintQuality = 0x400,
        Color = 0x800,
        Duplex = 0x1000,
        YResolution = 0x2000,
        TTOption = 0x4000,
        Collate = 0x8000,
        FormName = 0x10000,
        LogPixels = 0x20000,
        BitsPerPixel = 0x40000,
        PelsWidth = 0x80000,
        PelsHeight = 0x100000,
        DisplayFlags = 0x200000,
        DisplayFrequency = 0x400000,
        ICMMethod = 0x800000,
        ICMIntent = 0x1000000,
        MeduaType = 0x2000000,
        DitherType = 0x4000000,
        PanningWidth = 0x8000000,
        PanningHeight = 0x10000000,
        DisplayFixedOutput = 0x20000000
        }
    #endregion

    #region DEVMOD
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    struct DEVMODE
    {
        public const int CCHDEVICENAME = 32;
        public const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        [System.Runtime.InteropServices.FieldOffset(0)]
        public string dmDeviceName;
        [System.Runtime.InteropServices.FieldOffset(32)]
        public Int16 dmSpecVersion;
        [System.Runtime.InteropServices.FieldOffset(34)]
        public Int16 dmDriverVersion;
        [System.Runtime.InteropServices.FieldOffset(36)]
        public Int16 dmSize;
        [System.Runtime.InteropServices.FieldOffset(38)]
        public Int16 dmDriverExtra;
        [System.Runtime.InteropServices.FieldOffset(40)]
        public DM dmFields;

        [System.Runtime.InteropServices.FieldOffset(44)]
        Int16 dmOrientation;
        [System.Runtime.InteropServices.FieldOffset(46)]
        Int16 dmPaperSize;
        [System.Runtime.InteropServices.FieldOffset(48)]
        Int16 dmPaperLength;
        [System.Runtime.InteropServices.FieldOffset(50)]
        Int16 dmPaperWidth;
        [System.Runtime.InteropServices.FieldOffset(52)]
        Int16 dmScale;
        [System.Runtime.InteropServices.FieldOffset(54)]
        Int16 dmCopies;
        [System.Runtime.InteropServices.FieldOffset(56)]
        Int16 dmDefaultSource;
        [System.Runtime.InteropServices.FieldOffset(58)]
        Int16 dmPrintQuality;

        [System.Runtime.InteropServices.FieldOffset(44)]
        public POINTL dmPosition;
        [System.Runtime.InteropServices.FieldOffset(52)]
        public Int32 dmDisplayOrientation;
        [System.Runtime.InteropServices.FieldOffset(56)]
        public Int32 dmDisplayFixedOutput;

        [System.Runtime.InteropServices.FieldOffset(60)]
        public short dmColor; // See note below!
        [System.Runtime.InteropServices.FieldOffset(62)]
        public short dmDuplex; // See note below!
        [System.Runtime.InteropServices.FieldOffset(64)]
        public short dmYResolution;
        [System.Runtime.InteropServices.FieldOffset(66)]
        public short dmTTOption;
        [System.Runtime.InteropServices.FieldOffset(68)]
        public short dmCollate; // See note below!
        [System.Runtime.InteropServices.FieldOffset(70)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;
        [System.Runtime.InteropServices.FieldOffset(102)]
        public Int16 dmLogPixels;
        [System.Runtime.InteropServices.FieldOffset(104)]
        public Int32 dmBitsPerPel;
        [System.Runtime.InteropServices.FieldOffset(108)]
        public Int32 dmPelsWidth;
        [System.Runtime.InteropServices.FieldOffset(112)]
        public Int32 dmPelsHeight;
        [System.Runtime.InteropServices.FieldOffset(116)]
        public Int32 dmDisplayFlags;
        [System.Runtime.InteropServices.FieldOffset(116)]
        public Int32 dmNup;
        [System.Runtime.InteropServices.FieldOffset(120)]
        public Int32 dmDisplayFrequency;
    }
    #endregion

    #region POINTL
    struct POINTL
    {
        public Int32 x;
        public Int32 y;
    }

    #endregion




    public partial class ServerMain : Form
    {
        /*[DllImport("user32.dll")]
        static extern bool EnumDisplaySettingsEx(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode, uint dwFlags);
        */

        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        Socket listen = null;
        byte[] datas = new byte[200];
        delegate void StreamHandler();
        string ClientIP = "";




        public ServerMain()
        {
            InitializeComponent();
        }

        private void ServerMain_Load(object sender, EventArgs e)
        {
            string Host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostByName(Host);
            labelIP.Text = ip.AddressList[0].ToString();
            listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MessageBox.Show(ip.AddressList[0].ToString());
            listen.Bind(new IPEndPoint(IPAddress.Parse(ip.AddressList[0].ToString()), 8501));
            listen.Listen(1);
            listen.BeginAccept(new AsyncCallback(ConnectedCallback), null);

        }


        void ConnectedCallback(IAsyncResult iar)
        {
            Socket socket = listen.EndAccept(iar);
            socket.BeginReceive(datas, 0, datas.Length, SocketFlags.None, new AsyncCallback(dataReceived), socket);
        }


        void dataReceived(IAsyncResult iar)
        {
            Socket socket = (Socket)iar.AsyncState;
            int RDL = socket.EndReceive(iar);
            byte[] _data = new byte[RDL];
            Array.Copy(datas, _data, _data.Length);
            string action = Encoding.UTF8.GetString(_data);

            if(action.Contains("STREAM"))
            {
                ClientIP = action.Substring(0, action.IndexOf('|'));
                DEVMODE dv = new DEVMODE();
                EnumDisplaySettings(null, -1, ref dv);
                string resolution = dv.dmPelsWidth.ToString() + ":" + dv.dmPelsHeight.ToString() + "|";
                SendResolution(resolution);
                StreamHandler stream = new StreamHandler(StreamDesktop);
                stream.BeginInvoke(new AsyncCallback(ProcessEnded), null);
            } else
            {

            }
        }

        void SendResolution(string resolution)
        {
            Socket connect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            connect.Connect(IPAddress.Parse(ClientIP), 8501);
            byte[] encode = Encoding.UTF8.GetBytes(resolution);
            connect.Send(encode, 0, encode.Length, SocketFlags.None);
            connect.Close();
        }

        void StreamDesktop()
        {
            while(true)
            {
                Socket connect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connect.Connect(IPAddress.Parse(ClientIP), 8501);
                byte[] screen = ScreenShoot();
                connect.Send(screen, 0, screen.Length, SocketFlags.None);
                connect.Close();
            }
        }

        byte[] ScreenShoot()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics gr = Graphics.FromImage(bmp);

            gr.CopyFromScreen(0, 0, 0, 0, new Size(bmp.Width, bmp.Height));
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            return ms.GetBuffer();
        }

        void ProcessEnded(IAsyncResult iar)
        {

        }
    }
}

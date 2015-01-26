using System;
using System.IO;
using System.Text;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace examples
{
    public class Printer
    {
        public const short FILE_ATTRIBUTE_NORMAL = 0x80;
        public const short INVALID_HANDLE_VALUE = -1;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess,
            uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition,
            uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        
        private static string _LPTName = "LPT1";

        public static bool sendTextToLPT(string lptName, string receiptText)
        {
            _LPTName = lptName;
            return sendTextToLPT(receiptText);
        }

        public static bool sendTextToLPT(string receiptText)
        {
            IntPtr ptr = CreateFile(_LPTName, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

            if (ptr.ToInt32() == -1)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                return false;
            }
            else
            {
                try
                {
                    using (FileStream lpt = new FileStream(ptr, FileAccess.Write))
                    {
                        byte[] buffer = Encoding.ASCII.GetBytes(receiptText);
                        lpt.Write(buffer, 0, buffer.Length);
                        lpt.WriteByte((byte)'\r');
                        lpt.Close();
                        return true;
                    }
                }
                catch(Exception ex) { System.Windows.Forms.MessageBox.Show(ex.ToString()); return false; }
            }
        }
    }

    public enum CommReadMode
    { 
        ReadByte = 0,
        ReadChar = 1,
        ReadLine = 2,
        ReadExists = 3,
        ReadLimit = 4
    }

    public class CommPort
    {
        public string mPortName = "COM1";
        public int mBaudRate = 9600;
        public Parity mParity = Parity.None;
        public int mDataBits = 8;
        public StopBits mStopBits = StopBits.One;
        public event EventHandler DataReceived;
        public CommReadMode ReadMode = CommReadMode.ReadLine;
        public int ReadLength = 255;
        private SerialPort _CommPort;

        public CommPort()
        { }

        public bool IsOpen()
        {
            return (_CommPort == null) ? false : _CommPort.IsOpen;
        }

        public void Open()
        {
            if (_CommPort == null)
            {
                _CommPort = new SerialPort(mPortName, mBaudRate, mParity, mDataBits);
                _CommPort.ReceivedBytesThreshold = 1;
                _CommPort.DtrEnable = true;
                _CommPort.DataReceived += new SerialDataReceivedEventHandler(_CommPort_DataReceived);
                _CommPort.Open();
            }
        }

        void _CommPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(DataReceived == null) return;
            object obj = null;
            switch (ReadMode)
            {
                default: break;
                case CommReadMode.ReadByte: obj = _CommPort.ReadByte(); break;
                case CommReadMode.ReadChar: obj = _CommPort.ReadChar(); break;
                case CommReadMode.ReadLine: obj = _CommPort.ReadLine(); break;
                case CommReadMode.ReadExists: obj = _CommPort.ReadExisting(); break;
                case CommReadMode.ReadLimit:
                    {
                        byte[] bytes = new byte[ReadLength];
                        int len = _CommPort.Read(bytes, 0, bytes.Length);
                        byte[] sbytes = new byte[len];
                        for (int i = 0; i < len; i++) sbytes[i] = bytes[i];
                        obj = sbytes;
                    }
                    break;
            }
            DataReceived.DynamicInvoke( new object[] { obj, EventArgs.Empty });
        }

        public void Close()
        {
            if (_CommPort != null)
            {
                _CommPort.ReceivedBytesThreshold = 99999;
                _CommPort.DtrEnable = false;                
                _CommPort.DataReceived -= new SerialDataReceivedEventHandler(_CommPort_DataReceived);
                _CommPort.Close();
                _CommPort = null;
            }
        }
    }
}

using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace GeganetCom
{
    public class PoleDisplay
    {
        private const byte LCD_AC_AUTO_INCREMENT = 0x06;
        private const byte LCD_AC_AUTO_DECREASE = 0x04;
        private const byte LCD_MOVE_ENABLE = 0x05;
        private const byte LCD_MOVE_DISENABLE = 0x04;
        private const byte LCD_GO_HOME = 0x02; //AC=0,HOME?
        //?????????????
        private const byte LCD_DISPLAY_ON = 0x0C;
        private const byte LCD_DISPLAY_OFF = 0x08;
        private const byte LCD_CURSOR_ON = 0x0A;
        private const byte LCD_CURSOR_OFF = 0x08;
        private const byte LCD_CURSOR_BLINK_ON = 0x09;
        private const byte LCD_CURSOR_BLINK_OFF = 0x08;
        //???????,???DDRAM
        private const byte LCD_LEFT_MOVE = 0x18;
        private const byte LCD_RIGHT_MOVE = 0x1C;
        private const byte LCD_CURSOR_LEFT_MOVE = 0x10;
        private const byte LCD_CURSOR_RIGHT_MOVE = 0x14;
        //??????
        private const byte LCD_DISPLAY_DOUBLE_LINE = 0x38;
        private const byte LCD_DISPLAY_SINGLE_LINE = 0x30;
        private const byte LCD_CLEAR_SCREEN = 0X01;
        /***********************LCD1602????******************************/
        private const byte LINE1_HEAD = 0x80;   // DDRAM?
        private const byte LINE2_HEAD = 0xc0;   // DDRAM?
        private const byte LINE1 = 0;
        private const byte LINE2 = 1;
        private const byte LINE_LENGTH = 8;
        private const byte SYS_CR = 0x0D;
        private const char ZERO_CHAR = '0';

        private static int DisplayWidth = LINE_LENGTH;
        private static SerialPort _COMM;
        public static SerialPort COMM
        {
            get { return PoleDisplay._COMM; }
            set { PoleDisplay._COMM = value; }
        }

        ~PoleDisplay()
        {
            if (COMM.IsOpen) COMM.Close();
        }

        public static SerialPort InitComm(string portName, int baudRate)
        {
            return new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        }

        public static void WriteComm(double any)
        {
            if (_COMM == null) _COMM = new SerialPort("COM1", 2400, Parity.None, 8, StopBits.One);
            _COMM.Open();
            ClearScreen(5);
            FormatScreen(5);
            WriterR(any);
            _COMM.Close();
        }

        public static void WriterL(double too)
        {
            string str = too.ToString();            
            int d = DisplayWidth;            
            if (str.IndexOf('.') > -1) d++;
            if (str.Length > d) str = str.Substring(0, d);
            int l = str.Length;
            byte[] b = Encoding.ASCII.GetBytes(str);
            ClearScreen(5);
            _COMM.Write(b, 0, b.Length);
            Thread.Sleep(5);
        }

        public static void WriterR(double too)
        {
            string str = too.ToString();            
            int d = DisplayWidth;
            if (str.IndexOf('.') > -1) d++;
            if (str.Length > d) str = str.Substring(0, d);
            int l = str.Length;
            if ((d - l) > 0)
            {
                byte[] zb = new byte[(d - l)];
                for (int i = 0; i < zb.Length; i++)
                    zb[i] = (byte)PoleDisplay.ZERO_CHAR;
                _COMM.Write(zb, 0, zb.Length);
            }            
            byte[] b = Encoding.ASCII.GetBytes(str);
            ClearScreen(5);
            _COMM.Write(b, 0, b.Length);
            Thread.Sleep(5);
        }

        public static void ClearScreen(int delay)
        {
            if (_COMM != null && _COMM.IsOpen)
            {
                byte[] chr = new byte[] { PoleDisplay.LCD_DISPLAY_ON, PoleDisplay.SYS_CR, PoleDisplay.LCD_CLEAR_SCREEN, PoleDisplay.SYS_CR };
                _COMM.Write(chr, 0, chr.Length);
                if (delay > 0) Thread.Sleep(delay);
            }
        }

        public static void ClearDisplay()
        {
            if (_COMM != null)
            {
                if(!_COMM.IsOpen) _COMM.Open();
                ClearScreen(10);
                _COMM.Close();
            }
        }

        public static void FormatScreen(int delay)
        {
            if (_COMM != null && _COMM.IsOpen)
            {
                byte[] chrs = new byte[] { (PoleDisplay.LINE1_HEAD + PoleDisplay.LINE1), PoleDisplay.SYS_CR, PoleDisplay.LCD_LEFT_MOVE, PoleDisplay.SYS_CR };
                _COMM.Write(chrs, 0, chrs.Length);
                if (delay > 0) Thread.Sleep(delay);
            }
        }
    }
}

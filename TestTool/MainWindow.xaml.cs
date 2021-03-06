﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Forms;
using WII.HID.Lib;
using System.Threading;

namespace TestTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HIDDevice wiiMote;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Connectie openen met HID device met als vendor ID &H57E en als product ID &H306
            wiiMote = HIDDevice.GetHIDDevice(0x57E, 0x306);
            HIDReport reportW = wiiMote.CreateReport();
            reportW.ReportID = (byte)0x12;
            reportW.Data[1] = (byte)0x37;
            wiiMote.WriteReport(reportW);
            wiiMote.ReadReport(OnReadReport);
            Thread.Sleep(100);
            IRConfig();
        }
        private void OnReadReport(HIDReport report)
        {
            if (Thread.CurrentThread != Dispatcher.Thread)
            {
                this.Dispatcher.Invoke(new ReadReportCallback(OnReadReport), report);
            }
            else
            {
                switch (report.ReportID)
                {
                    case 0x20:
                        // Status report
                        Console.WriteLine("0x20");
                        Console.WriteLine(report.Data[0]);
                        LeesKnop(report);
                        break;
                    case 0x21:
                        // Memory en register read report
                        Console.WriteLine("0x21");
                        Console.WriteLine(report.Data.Length);
                        LeesKnop(report);
                        break;
                    case 0x22:
                        // Acknowledge report
                        Console.WriteLine("0x22");
                        Console.WriteLine(report.Data.Length);
                        LeesKnop(report);
                        break;
                    case 0x30:
                        // Core buttons data report
                        Console.WriteLine("0x30");
                        Console.WriteLine(report.Data.Length);
                        LeesKnop(report);
                        break;
                    case 0x31:
                        Console.WriteLine("0x31");
                        Console.WriteLine(report.Data.Length);
                        LeesKnop(report);
                        // Core buttons en accelerometer data report
                        break;
                    case 0x33:
                        Console.WriteLine("0x33");
                        Console.WriteLine(report.Data.Length);
                        LeesKnop(report);
                        // Core buttons en accelerometer data report
                        break;
                    case 0x37:
                        Console.WriteLine("0x37");
                        LeesKnop(report);
                        LocateIR(report.Data);
                        break;
                }
                wiiMote.ReadReport(OnReadReport);
                Thread.Sleep(100);
            }
        }
        private void LeesKnop(HIDReport report)
        {
            if ((report.Data[1] & (byte)0x08) != 0)
            {
                Console.WriteLine("ok");
                txtA.Text=("ok");
            }
            else if ((report.Data[1] & (byte)0x04) != 0)
            {
                txtB.Text = ("ok");
            }
            else if ((report.Data[1] & (byte)0x10) != 0)
            {
                txtMin.Text = ("ok");
            }
            else if ((report.Data[1] & (byte)0x80) != 0)
            {
                txtHome.Text = ("ok");
            }
            else if ((report.Data[0] & (byte)0x10) != 0)
            {
                txtPlus.Text = ("ok");
            }
            else if ((report.Data[1] & (byte)0x02) != 0)
            {
                txt1.Text = ("ok");
            }
            else if ((report.Data[1] & (byte)0x01) != 0)
            {
                txt2.Text = ("ok");
            }
            else if ((report.Data[0] & (byte)0x08) != 0)
            {
                txtUp.Text = ("ok");
            }
            else if ((report.Data[0] & (byte)0x04) != 0)
            {
                txtDown.Text = ("ok");
            }
            else if ((report.Data[0] & (byte)0x01) != 0)
            {
                txtLeft.Text = ("ok");
            }
            else if ((report.Data[0] & (byte)0x02) != 0)
            {
                txtRight.Text = ("ok");
            }
        }
        private void WriteData(int address, byte[] data)
        {
            if (wiiMote != null)
            {
                int index = 0;
                while (index < data.Length)
                {
                    int leftOver = data.Length - index;
                    int count = (leftOver > 16 ? 16 : leftOver);
                    int tempAddress = address + index;
                    HIDReport report = wiiMote.CreateReport();
                    report.ReportID = 0x16;
                    report.Data[0] = (byte)((tempAddress & 0x4000000) >> 0x18);
                    report.Data[1] = (byte)((tempAddress & 0xff0000) >> 0x10);
                    report.Data[2] = (byte)((tempAddress & 0xff00) >> 0x8);
                    report.Data[3] = (byte)((tempAddress & 0xff));
                    report.Data[4] = (byte)count;
                    Buffer.BlockCopy(data, index, report.Data, 5, count);
                    wiiMote.WriteReport(report);
                    index += 16;
                }
            }
        }

        private void IRConfig()
        {
            HIDReport reportI = wiiMote.CreateReport();
            reportI.ReportID = 0x13;
            reportI.Data[0] = (byte)0x04;
            wiiMote.WriteReport(reportI);
            Thread.Sleep(50);
            HIDReport report2 = wiiMote.CreateReport();
            report2.ReportID = 0x1A;
            report2.Data[0] = (byte)0x04;
            wiiMote.WriteReport(report2);
            Thread.Sleep(50);
            byte[] data = new byte[] { 0x08 };
            WriteData(0xB00030, data);
            Thread.Sleep(50);
            byte[] data2 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x90, 0x00, 0x41 };
            WriteData(0xB00000, data2);
            Thread.Sleep(50);
            byte[] data3 = new byte[] { 0x40, 0x00 };
            WriteData(0xB0001A, data3);
            Thread.Sleep(50);
            byte[] data4 = new byte[] { 0x01 };
            WriteData(0xB00033, data4);
            Thread.Sleep(50);
            byte[] data5 = new byte[] { 0x08 };
            WriteData(0xB00030, data5);
        }
        private void LocateIR(byte[] data)
        {
            //in 2 delen
            int X1Y1X2Y2 = data[7];
            string loc = Convert.ToString(X1Y1X2Y2, 2);
            while (loc.Length < 8)
            {
                loc = "0" + loc;
            }
            string sY1 = loc.Substring(0, 2) + "00000000";
            string sX1 = loc.Substring(2, 2) + "00000000";
            string sY2 = loc.Substring(4, 2) + "00000000";
            string sX2 = loc.Substring(6) + "00000000";
            float X1 = data[5] + Convert.ToInt32(sX1, 2);
            float Y1 = data[6] + Convert.ToInt32(sY1, 2);
            float X2 = data[8] + Convert.ToInt32(sX2, 2);
            float Y2 = data[9] + Convert.ToInt32(sY2, 2);
            int X3Y3X4Y4 = data[12];
            string loc2 = Convert.ToString(X3Y3X4Y4, 2);
            while (loc2.Length < 8)
            {
                loc2 = "0" + loc;
            }
            string sY3 = loc.Substring(0, 2) + "00000000";
            string sX3 = loc.Substring(2, 2) + "00000000";
            string sY4 = loc.Substring(4, 2) + "00000000";
            string sX4 = loc.Substring(6) + "00000000";
            float X3 = data[10] + Convert.ToInt32(sX3, 2);
            float Y3 = data[11] + Convert.ToInt32(sY3, 2);
            float X4 = data[13] + Convert.ToInt32(sX4, 2);
            float Y4 = data[14] + Convert.ToInt32(sY4, 2);
            X1 /= 1023; Y1 /= 765;
            X2 /= 1023; Y2 /= 765;
            X3 /= 1023; Y3 /= 765;
            X4 /= 1023; Y4 /= 765;
        }
    }
}

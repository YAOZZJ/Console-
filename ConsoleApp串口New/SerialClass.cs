using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace UsrCommunication
{
    #region 全局串口事件数据
    /// <summary>
    /// 全局串口事件数据
    /// </summary>
    public class SerialPortEventArgs
    {
        public bool isOpened = false;
        public Byte[] receiveBytes = null;
    }
    #endregion

    //***************************************************************************************
    #region 委托定义
    /// <summary>
    /// 委托定义
    /// 传送isOpened& recedBytes数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SerialPortEventHandler(object sender, SerialPortEventArgs e);


    #endregion
    //***************************************************************************************
    public class SerialClass
    {
        #region 事件&数据
        private SerialPort _serialPort = null;
        private object LockReceiving = new object();
        public int sendBytesCount = 0;
        public int receiveBytesCount = 0;

        //初始无订阅事件状态
        public event SerialPortEventHandler ComReceiveDataEvent = null;//接收数据事件
        public event SerialPortEventHandler ComOpenEvent = null;//串口打开事件
        public event SerialPortEventHandler ComCloseEvent = null;//串口关闭事件

        #endregion
        #region 构造函数
        public SerialClass()
        {
            _serialPort = new SerialPort();
        }
        /// <summary>
        /// 9600, Parity.None, 8, StopBits.One
        /// </summary>
        /// <param name="PortName">端口号</param>
        public SerialClass(string portNumber)
        {
            _serialPort = new SerialPort(portNumber);
            _serialPort.RtsEnable = true;
            _serialPort.ReadTimeout = 2000;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portName">端口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">奇偶校验</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        public SerialClass(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPort.RtsEnable = true;  //自动请求
            _serialPort.ReadTimeout = 3000;//超时
        }

        #endregion

        public string[] GetPortName()
        {
            return SerialPort.GetPortNames();
        }
        /// <summary>
        /// 串口打开
        /// </summary>
        public void SeriPortOpen()
        {
            SerialPortEventArgs Args = new SerialPortEventArgs();
            if (_serialPort.IsOpen == false)
            {
                try
                {
                    _serialPort.Open();
                    SerialPortSetting();
                    if (ComOpenEvent != null)
                    {
                        ComOpenEvent(this, Args);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        /// <summary>
        /// 串口关闭,串口关闭可能会导致互锁
        /// 用多线程关闭
        /// </summary>
        public void SerialPortClose()
        {
            Thread closeThread = new Thread(new ThreadStart(SerialPortCloseThread));
            closeThread.Start();
        }
        private void SerialPortCloseThread()
        {
            SerialPortEventArgs Args = new SerialPortEventArgs();
            Args.isOpened = false;
            try
            {
                _serialPort.Close();
                _serialPort.DataReceived -= SerialPortDataReceived;
            }
            catch (System.Exception)
            {
                Args.isOpened = true;
            }
            if (ComCloseEvent != null)
            {
                ComCloseEvent(this, Args);
            }
        }
        private void SerialPortSetting()
        {
            _serialPort.DataReceived += SerialPortDataReceived;
        }

        /// <summary>
        /// 串口发送,byte[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns>发送是否成功</returns>
        public bool SerialPortSend(byte[] data)
        {
            if (!_serialPort.IsOpen)
            {
                return false;
            }
            try
            {
                _serialPort.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort.BytesToRead <= 0) return;
            lock (LockReceiving)//不保证任何实例成员都是线程安全的
            {
                int len = _serialPort.BytesToRead;
                byte[] data = new byte[len];
                try
                {
                    _serialPort.Read(data, 0, len);
                }
                catch (System.Exception)
                {

                }
                SerialPortEventArgs Args = new SerialPortEventArgs();
                Args.receiveBytes = data;
                if (ComReceiveDataEvent != null)
                {
                    ComReceiveDataEvent(this, Args);

                }
                receiveBytesCount += len;
                //Console.WriteLine(receiveBytesCount);
            }
        }

    }
}

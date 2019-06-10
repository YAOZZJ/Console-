using System;
using UsrCommunication;
namespace MainProgram
{
    class Program
    {
        
        static void Main(string[] args)
        {
            SerialClass SerialPort1 = new SerialClass("COM1");
            //SerialPort1.GetPortName();
            foreach (string str1 in SerialPort1.GetPortName())
            {
                Console.WriteLine(str1);
            }
            Console.ReadKey();
        }
    }
}

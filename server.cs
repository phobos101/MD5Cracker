using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;

namespace server
{
    public class Server
    {
        public string fileName = "c:\\09002866-MD5.txt";
        public string readText = "";
        public string timeText = "";
        public string hashValue = "";
        public string returnData = "";
        public string sendData = "";
        public string clearText = null;

        public DateTime timeStart;
        public DateTime timeEnd;
        public DateTime timeInterval;
        public TimeSpan duration;

        public StreamWriter SWwriter;
        public StreamReader SWreader;

        public int start, end, tmp = 100000;

        public UdpClient udpClientSend = new UdpClient();
        public UdpClient udpClientRecieve = new UdpClient(8888);

        public Byte[] sendBytes = new Byte[1024]; // buffer to read the data into 1 kilobyte at a time
        public Byte[] recieveBytes = new Byte[1024]; // buffer to read the data into 1 kilobyte at a time

        public IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 8888);  //open port 8888 on this machine
        public IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 8888);

        static void Main(string[] args)
        {
            //Get Local IP Address
            string host = Dns.GetHostName();
            IPHostEntry ip = Dns.GetHostEntry(host);

            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("-           Welcome to Rob's MD5 cracker        -");
            Console.WriteLine("-                   ---SERVER---                -");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("-             Student Number: 09002866          -");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Server IP: " + ip.AddressList[0].ToString());
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Hash must be entered BEFORE clients connect!");
            Console.WriteLine();
            Console.WriteLine();
            Server server = new Server();
            server.hash();
        }

        public void hash()
        {
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("-         Please enter MD5 hash value below     -");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine();
            hashValue = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine("Clients may now connect");
            Console.WriteLine();

            control();
        }

        public void control()
        {
            //Start from where the program left off if server crashes
            if (File.Exists(fileName))
            {
                SWreader = new StreamReader(fileName);
                
                //reads in the time of the first request
                timeText = SWreader.ReadLine();
                timeStart = DateTime.Parse(timeText.Split()[7]);

                //reads the last line
                while (SWreader.Peek() >= 0)
                {
                    readText = SWreader.ReadLine();
                }
                start = Int32.Parse(readText.Split()[2]);
                end = Int32.Parse(readText.Split()[4]);                

                SWreader.Close();
                SWwriter = new StreamWriter(fileName, true);
            }
            else
            {
                start = 0;
                end = start + 100000;

                SWwriter = new StreamWriter(fileName, false);
            }

            while (returnData.TrimEnd() != "stop")
            {
                recieveBytes = udpClientRecieve.Receive(ref remoteIPEndPoint);
                returnData = Encoding.ASCII.GetString(recieveBytes);

                if (returnData.TrimEnd() == "hello")
                {
                    //Writes to screen that a client has connected and the IP address of client
                    Console.WriteLine();
                    Console.WriteLine("Client connected from " + remoteIPEndPoint.Address.ToString());
                    Console.WriteLine();

                    //Get IP address of new client
                    IPAddress address = IPAddress.Parse(remoteIPEndPoint.Address.ToString());

                    //send reply to the client
                    udpClientSend.Connect(address, 7777); //open a connection to that location 
                    sendBytes = Encoding.ASCII.GetBytes("Reply from server recieved!".PadRight(1024));
                    udpClientSend.Send(sendBytes, sendBytes.GetLength(0));

                    //Establish the new client 
                    sendData = hashValue + "," + start + "," + end;
                    sendBytes = Encoding.ASCII.GetBytes(sendData.TrimEnd());
                    udpClientSend.Send(sendBytes, sendBytes.GetLength(0));
                    timeInterval = DateTime.Now;

                    if (start == 0)
                    {
                        timeStart = DateTime.Now;
                        Console.WriteLine();
                        Console.WriteLine("Timer started!");
                        Console.WriteLine();
                    }

                    SWwriter.WriteLine(remoteIPEndPoint.Address.ToString() + " received " + start + " - " + end + " at " +timeInterval);
                    SWwriter.Flush();

                    start = end;
                    end = end + tmp;
                    returnData = "";                    
                }

                else if (returnData.TrimEnd() == "more")
                {
                    IPAddress address2 = IPAddress.Parse(remoteIPEndPoint.Address.ToString());

                    udpClientSend.Connect(address2, 7777);
                    sendData = start + "," + end;
                    sendBytes = Encoding.ASCII.GetBytes(sendData.TrimEnd());
                    udpClientSend.Send(sendBytes, sendBytes.GetLength(0));
                    timeInterval = DateTime.Now;

                    SWwriter.WriteLine(remoteIPEndPoint.Address.ToString() + " received " + start + " - " + end + " at " + timeInterval);
                    SWwriter.Flush();

                    start = end;
                    end = end + tmp;
                    returnData = "";

                    if (start % 1000000 == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Upto " + (start / 1000000) + " million checked...");
                    }
                }
            } // End of While loop

            //Recieve the cleartext from the client and write to screen
            Console.WriteLine();
            recieveBytes = udpClientRecieve.Receive(ref remoteIPEndPoint);
            clearText = Encoding.ASCII.GetString(recieveBytes);
            Console.WriteLine(clearText.TrimEnd());

            //Extrapolate the time taken to crack the cipher
            timeEnd = DateTime.Now; //End point of the timer
            duration = timeEnd - timeStart; //working out the duration of the operation

            SWwriter.WriteLine();
            SWwriter.WriteLine("Cleartext found: " + clearText);
            SWwriter.WriteLine();
            SWwriter.WriteLine("Time taken: " + duration);
            SWwriter.Close();
           
            //Broadcast to clients to say the MD5 has been cracked
            IPAddress broadcastAddress = IPAddress.Parse(broadcastEndPoint.Address.ToString());
            udpClientSend.Connect(broadcastAddress, 7777);
            sendBytes = Encoding.ASCII.GetBytes("found");
            udpClientSend.Send(sendBytes, sendBytes.GetLength(0)); 
            
            //End the program telling the user the time taken to complete
            Console.WriteLine();
            Console.WriteLine("Time taken: " + duration); //Displays the time taken to crack
            Console.WriteLine();
            Console.WriteLine("Press any key to quit");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}

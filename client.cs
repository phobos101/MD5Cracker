using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Threading;
using System.Timers;

namespace client
{
    class client
    {
        private static System.Timers.Timer aTimer = new System.Timers.Timer(3000);        
               
        public string ServerAddress = string.Empty;
        public string hashValue;
        public string recieveData = "";
        public string allValues = "";

        public int start;
        public int end;
        public int proccessors = Environment.ProcessorCount;

        public UdpClient udpClientSend = new UdpClient();
        public UdpClient udpClientRecieve = new UdpClient(7777);

        public Byte[] sendBytes = new Byte[1024]; // buffer to send the data into 1 kilobyte at a time
        public Byte[] recieveBytes = new Byte[1024]; // buffer to read the data into 1 kilobyte at a time

        public IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 7777);        
       
        static void Main(string[] args)
        {
            //Introduction to the client program
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("-          Welcome to Rob's MD5 cracker         -");
            Console.WriteLine("-                 ---CLIENT---                  -");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("-             Student Number: 09002866          -");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Please ensure that the server has started and ");
            Console.WriteLine("that the hash has been entered before continuing");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Once the server IP address is entered, the program");
            Console.WriteLine("will retrieve all the neccesary data. No further user");
            Console.WriteLine("input will be neccesary until the MD5 key is cracked");
            Console.WriteLine();
            Console.WriteLine();

            //Call the first function to test network connectivity to the server.
            client client = new client();
            client.connect();
        }

        public void connect() //This section of code ensures network connectivity to the server
        {
            Console.WriteLine("What is the IP Address of the server? ");
            Console.WriteLine();
            ServerAddress = Console.ReadLine();
                                    
            IPAddress remoteAddr = Dns.GetHostEntry(ServerAddress).AddressList[0];

            Ping ping = new Ping();
            PingReply reply = ping.Send(remoteAddr.ToString(), 100);

            Console.WriteLine();

            if (reply.Status == IPStatus.Success)
            {
                Console.WriteLine("Connection sucessfully established to " + ServerAddress);
                Console.WriteLine();
                hello();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Cannot connect to " + ServerAddress);
                Console.WriteLine();
                Console.WriteLine("Returning you to program start.");
                Console.ReadLine();
                connect();
            }
            Console.ReadLine();
        }

        public void hello() //This section of code is executed on a successful network connection to the specified IP address, it sends a hello packet to ensure the server and client can communicate.
        {
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Enabled = true;

            Console.WriteLine();
            Console.WriteLine("Sending hello message to the server...");

            //send hello
            udpClientSend.Connect(ServerAddress, 8888);  //IP Address of the server
            sendBytes = Encoding.ASCII.GetBytes("hello".PadRight(1024));
            udpClientSend.Send(sendBytes, sendBytes.GetLength(0));  //Send the packet

            //recieve reply
            recieveBytes = udpClientRecieve.Receive(ref remoteIPEndPoint);
            recieveData = Encoding.ASCII.GetString(recieveBytes);
            Console.WriteLine(recieveData.TrimEnd()); //output to screen

            aTimer.Enabled = false;

            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("-           CONNECTION ESTABLISHED              -");
            Console.WriteLine("-      PROCCEDING TO COMPUTE HASH VALUE         -");
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine();

            //Recieve MD5 hash
            recieveBytes = udpClientRecieve.Receive(ref remoteIPEndPoint);
            recieveData = Encoding.ASCII.GetString(recieveBytes);
            allValues = recieveData;

            hashValue = allValues.Split(',')[0];
            start = Int32.Parse(allValues.Split(',')[1]);
            end = Int32.Parse(allValues.Split(',')[2]);

            Console.WriteLine("Hash value recieved: " + hashValue);
            Console.WriteLine();

            compute();                
        }

        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            Console.WriteLine();
            Console.WriteLine("No response from server...");
            Console.WriteLine("Ensure the server is running with the MD5 hash entered.");
            Console.WriteLine();
            Console.WriteLine("Press any key to quit");
            Console.ReadLine();
            Environment.Exit(0);
        }

        public void compute() //This code section computes the hash value it is assigned and returns the data to the server
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            int count = 0;
            int stop = 0;
            string tocheck = "";
            string updatedValues = "";

            while (stop != 1)
            {
                for (count = start; count < end; count++)
                {
                    tocheck = count.ToString();
                    if (hashValue.CompareTo(generateHash(tocheck)) == 0)
                    {
                        //send stop
                        stop = 1;
                        sendBytes = Encoding.ASCII.GetBytes("stop");
                        udpClientSend.Send(sendBytes, sendBytes.GetLength(0));

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Ciphertext has been cracked! The clear text is " + tocheck);
                        Console.WriteLine();
                        Console.WriteLine("Sending result to server");
                        count = end;

                        //send result
                        sendBytes = Encoding.ASCII.GetBytes("Ciphertext has been cracked! The clear text is " + tocheck);
                        udpClientSend.Send(sendBytes, sendBytes.GetLength(0));

                        Console.WriteLine();
                        Console.WriteLine("Press any key to quit");
                        Console.ReadLine();
                        Environment.Exit(0);
                    }

                    if (count % 100000 == 0)
                    {
                        Console.WriteLine("Computing " + start + " - " + end);
                    }
                } //end of the FOR Loop

                sendBytes = Encoding.ASCII.GetBytes("more");
                udpClientSend.Send(sendBytes, sendBytes.GetLength(0));

                recieveBytes = udpClientRecieve.Receive(ref remoteIPEndPoint);
                updatedValues = Encoding.ASCII.GetString(recieveBytes);

                if (updatedValues == "found")
                {
                    Console.WriteLine();
                    Console.WriteLine("Ciphertext cracked by another client");
                    Console.WriteLine("Press any key to quit");
                    Console.ReadLine();

                    Environment.Exit(0);
                }

                else
                {
                    start = Int32.Parse(updatedValues.Split(',')[0]);
                    end = Int32.Parse(updatedValues.Split(',')[1]);
                }
            } //end of WHILE loop
        } //end of Compute 

        static string generateHash(string input)
        {
            //the method used here to generate the MD5 hash is a standard method provided by Microsoft
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}


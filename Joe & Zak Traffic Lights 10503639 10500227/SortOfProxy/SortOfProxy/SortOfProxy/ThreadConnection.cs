//************************************************************************//
// This project makes an extremely simple listening TCP/IP socket.        //
// By Nigel.                                                              //
//                                                                        //
// For what it does, this project looks (and probably is) extremely       //
// overcomplicated.  In particular, it creates Threads all over the place.//
//                                                                        //
// Please use this code, such as it is,  for any eduactional or non       //
// profit making research porposes on the conditions that.                //
//                                                                        //
// 1.   You may only use it for educational and related research          //
//      pusposes.                                                         //
//                                                                        //
// 2.   You leave my name on it.                                          //
//                                                                        //
// 3.   You correct at least 10% of the typig and spekking mistskes.      //
//                                                                        //
// © Nigel Barlow nigel@soc.plymouth.ac.uk 2012                           //
//************************************************************************//

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;




namespace SortOfProxy
{



    class ThreadConnection
    {
        NetworkStream           connectionStream = null;
        BinaryReader            inStream         = null;
        BinaryWriter            outStream        = null;
        Socket                  connection       = null;
        Proxy                   owner            = null;      
        int                     bufferSize       = 200;
        byte[]                  remoteAddress    = new byte[4];
        bool                    running          = true;


        //*******************************************************************//
        // Constructor.  All the action kicks off here.   create a listening //
        // socket.                                                           //
        //*******************************************************************//
        public ThreadConnection(Socket newConnection, Proxy owner)
        {
            connection       = newConnection;
            connectionStream = new NetworkStream(connection);
            outStream        = new BinaryWriter(connectionStream);
            inStream         = new BinaryReader(connectionStream);
            this.owner       = owner;
        }



        //*******************************************************************//
        // When we turn ourselves into a Thread, this method will treated by //
        // the low level schedluer as a separate process.  It runs in its own//
        // time, but our memory space.                                       //
        //*******************************************************************//
        public void run()
        {
            owner.AddConnection(this); // Is this threadsafe?  It is now!

            Console.WriteLine("Another connection Thread running");

            try
            {

                while (running)
                {             
                    byte[] packet = new byte[bufferSize];

                    int bytes = inStream.Read(packet, 0, bufferSize);

                    //******************************************************//
                    // Horroble hack; if the number of bytes read is 0 we   //
                    // have probably been disconnected.                     //
                    //******************************************************//
                    if (bytes == 0)  throw new Exception("Disconnected");

                    //*******************************************************//
                    //Does any other connection have the IP number that this //
                    //packet was to be sent to?  If it does, transmit the    //
                    //packet.                                                //
                    //*******************************************************//
                    String s = connection.RemoteEndPoint.ToString();

                    Console.WriteLine("Data arrived from " + s);

                    //*******************************************************//
                    // To assist with debugging only wait one second (which  //
                    // is a loooong time) and log a console message if timed //
                    // out.                                                  //
                    //*******************************************************//
                    bool timedOut = !owner.sem.WaitOne(1000);
                    if (timedOut)
                        Console.WriteLine("Wait timed out in connect thread run()");


                    //*******************************************************//
                    // Iterate through the list of connections and relay the //
                    // packet to any IP we recognise.                        //
                    //*******************************************************//
                    foreach (ThreadConnection conn in owner.listConnections)
                    {
                        String[] remoteIpStrs = conn.connection.RemoteEndPoint.ToString().Split('.');
                        byte[] remoteIpBytes = new byte[4];

                        remoteIpBytes[0] = Byte.Parse(remoteIpStrs[0]);
                        remoteIpBytes[1] = Byte.Parse(remoteIpStrs[1]);
                        remoteIpBytes[2] = Byte.Parse(remoteIpStrs[2]);
                        remoteIpBytes[3] = Byte.Parse(remoteIpStrs[3].Split(':')[0]);

                        if ((remoteIpBytes[0] == packet[0])
                            && (remoteIpBytes[1] == packet[1])
                            && (remoteIpBytes[2] == packet[2])
                            && (remoteIpBytes[3] == packet[3]))
                        {
                            //**************************************************//
                            // Copy the "from" IP into the packet.              //
                            //**************************************************//
                            for (int i = 0; i < 4; i++) packet[i] = remoteIpBytes[i];

                            conn.connection.Send(packet);
                            Console.WriteLine("Re transmitted packet to "
                                + remoteIpBytes[0] + "."
                                + remoteIpBytes[1] + "."
                                + remoteIpBytes[2] + "."
                                + remoteIpBytes[3]);
                        }

                    }   // For


                    try
                    {
                        owner.sem.Release();
                    }
                    catch (Exception ee) { }

                }
            }
            catch (Exception doh)
            {
                Console.Out.WriteLine("Unexpected exception.");
                Console.Out.WriteLine("Error caused by " + doh.Message);
                connection.Close();
                running = false;
                owner.RemoveConnection(this);
                try
                {
                    owner.sem.Release();
                }
                catch (Exception ee) { }
            }
        }
    }
}

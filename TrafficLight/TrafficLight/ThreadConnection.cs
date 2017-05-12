using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace TrafficLight
{


    //************************************************************************//
    // This class represents a thread to manage a network connection.         //  
    //                                                                        //
    // For what it does, this project looks (and probably is) extremely       //
    // overcomplicated.                                                       //
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
    // © Nigel Barlow nigel@soc.plymouth.ac.uk 2016                           //
    //************************************************************************//

    class ThreadConnection
    {

        //*******************************************************************//
        // Class instance variables.                                         //
        //*******************************************************************//
        NetworkStream    connectionStream = null;
        FormTrafficLight owner            = null;
        BinaryReader     inStream         = null;
        BinaryWriter     outStream        = null;
        TcpClient        connection       = null;
        int              bufferSize       = 200;
        byte[]           remoteAddress    = new byte[4];
        bool             running          = true;
        
        
        //*******************************************************************//
        // This one is needed so that we can post messages back to the form's//
        // thread and don't violate C#'s threading rile that says you can    //
        // only touch the UI components from the form's thread.              //
        //*******************************************************************//
        SynchronizationContext uiContext = null;


        //*******************************************************************//
        // Constructor.  All the action kicks off here.   create a listening //
        // socket.                                                           //
        //*******************************************************************//
        public ThreadConnection(SynchronizationContext context, 
                                TcpClient              newConnection, 
                                FormTrafficLight       owner)
        {
            connection       = newConnection;
            connectionStream = connection.GetStream();
            this.owner       = owner;
            uiContext        = context;
            outStream        = new BinaryWriter(connectionStream);
            inStream         = new BinaryReader(connectionStream);
          }



        //*******************************************************************//
        // When we turn ourselves into a Thread, this method will treated by //
        // the low level schedluer as a separate process.  It runs in its own//
        // time, but our memory space.                                       //
        //*******************************************************************//
        public void Run()
        {

            Console.WriteLine("Another connection Thread running");

            try
            {

                while (running)
                {
                    //******************************************************//
                    // This blocks (sits and waits) until there is some     //
                    // to read.                                             //
                    //******************************************************//
                    byte[] packet = new byte[bufferSize];
                    inStream.Read(packet, 0, bufferSize);


                    //******************************************************//
                    // Get the first 4 bytes from the packet we recieved.   //
                    // These represent the IP number of the sender.         //
                    //******************************************************//
                    byte ip1 = packet[0];
                    byte ip2 = packet[1];
                    byte ip3 = packet[2];
                    byte ip4 = packet[3];


                    //******************************************************//
                    // Get the first 4 bytes from the packet we recieved.   //
                    // These represent the IP number of the sender.         //
                    //******************************************************//
                    String remoteIP = System.Convert.ToString(ip1)  //Reconstrict IP
                                      + "."
                                      + System.Convert.ToString(ip2)
                                      + "."
                                      + System.Convert.ToString(ip3)
                                      + "."
                                      + System.Convert.ToString(ip4);


                    char[] characters = new char[bufferSize];


                    //**************************************************************//
                    // Iterate through the buffer, assemble a character array.      //
                    //**************************************************************//
                    int numChars    = 0;                    //Count how long the string is
                    int bufferIndex = 4;                    //Start assembling message
                    for (int i = 0; i < bufferSize - 4; i++)
                    {
                        characters[i] = (char)packet[bufferIndex++];
                        numChars++;
                        if (characters[i] == 0) break;
                    }


                    String stringFromServer = new String(characters, 0, numChars);

                    String message = remoteIP + " sent " + stringFromServer;

                    Console.WriteLine(message);

                    //**************************************************************//
                    // Post the messare we have received back to the form.  Posting //
                    // messages to ourself; how sad is that?  the significance is   //
                    // that the messages come back in the form's main thread.       //
                    //**************************************************************//
                    uiContext.Post(owner.MessageReceived, message);
                }


            }
            catch (Exception doh)
            {
                Console.Out.WriteLine("Unexpected exception.");
                Console.Out.WriteLine("Error caused by " + doh.Message);
                running = false;
            }
        }



        //*******************************************************************//
        // Stop the thread and close the connection.                         //
        //*******************************************************************//
        public void StopThread()
        {
            running = false;
            connection.GetStream().Close();
            connection.Close();
            
            Console.WriteLine("Connection thread stopped");
        }


    }   // End of classy class.
}       // End of namespace

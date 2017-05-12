//************************************************************************//
// This project makes an extremely simple listening TCP/IP socket.        //
// By Nigel.                                                              //
//                                                                        //
// Soft of prroxy server.   Comsole only, no user interface.              //                 
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
// © Nigel Barlow nigel@soc.plymouth.ac.uk 2017                           //
//************************************************************************//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace SortOfProxy
{
    class Proxy
    {

        //*******************************************************************//
        // Nigel's instance variables.                                       //
        //*******************************************************************//

        private TcpListener            listeningSocket = null;   //Listening socket.
        private Socket connection = null;   //TCP/IP socket to handle the actual connection
        private int port = 5000;   //Well away from standard Internet ports.
        private IPHostEntry localHostInfo = null;
        public List<ThreadConnection> listConnections = new List<ThreadConnection>();


        //******************************************************************//
        // You know (or will do) wat a Semaphore is.  Use one to protect    //
        // the list of connections, which is not Threadsafe.  You could     //
        // use the C# word "lock" instead.                                  //
        //                                                                  //
        // The 1 and 1 in the parameter list represent the initial and      //
        // maximuum value of the semaphore.                                 //
        // Oops, a puvlic instance variable ... shhhh.                      //
        //******************************************************************//
        public Semaphore sem = new Semaphore(1, 1);





        //******************************************************************//
        // Make an instance of ourself and run it.  Some of you may have    //
        // not seen this style.                                             //
        //******************************************************************//
        static void Main(string[] args)
        {
            Proxy Proxy = new Proxy();
            Proxy.StartProxy();
        }
      
    
  


        //******************************************************************//
        // Start the sort of proxy.                                         //
        //******************************************************************//
        private void StartProxy()
        {

            //**************************************************************//
            // Try to create a listening socket.                            //
            //**************************************************************//
            try
            {
                listeningSocket = new TcpListener(System.Net.IPAddress.Any, port);
                listeningSocket.Start();
                Console.WriteLine("Listening socket started");  //Why don't these echo!
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message, "Error opening listening socket");
                return;
            }




            //**************************************************************//
            // This next bit just prints out the IP address and the host.   //
            // There must be a neater way to do this - Nigel.               //
            //**************************************************************//
            Console.Out.WriteLine("Listener running");

            localHostInfo = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            foreach (IPAddress address in localHostInfo.AddressList)
                Console.WriteLine("I have Ip address " + address.ToString());






            //**************************************************************//
            // Sit and wait until something attempts to connect to us.      //
            //                                                              //
            // We block (i.e. wait) on the listeningSocket.AcceptSocket();  //
            // When it does, we get passed a new connection.                //
            //                                                              //
            // This run() method (I have called it run() to make C# look    //
            // like Java) You will find that this form (Thread) won't       //
            // respond wo any Windows events (try it), as it is blocking in //
            // the code below.                                              //
            //**************************************************************//

            while (true)
            {
                connection = listeningSocket.AcceptSocket();

                //***********************************************************//
                //We have now accepted a connection.                         //
                //                                                           //
                //There are several ways to do this next bit.   Here I make a//
                //network stream and use it to create two other streams, an  //
                //input and an output stream.   Life gets easier at that     //
                //point.                                                     //
                //***********************************************************//
                ThreadConnection threadConnection = new ThreadConnection(connection, this);



                //***********************************************************//
                // Create a new Thread to manage the connection that receives//
                // data.  If you are a Java programmer, this looks like a    //
                // load of hokum cokum..                                     //
                //***********************************************************//
                Thread threadRunner = new Thread(new ThreadStart(threadConnection.run));
                threadRunner.Start();

                Console.WriteLine("Created new connection class");
            }
        }



        //******************************************************************//
        // Add a new connection to the list of connections in a threadsafe  //
        // way.                                                             //
        //******************************************************************//
        public void AddConnection(ThreadConnection newConnection)
        {
            //*******************************************************//
            // To assist with debugging only wait one second (which  //
            // is a loooong time) and log a console message if timed //
            // out.                                                  //
            //*******************************************************//
            bool timedOut = sem.WaitOne(1000);
            if (timedOut)
                Console.WriteLine("Wait timed out in connect thread run()");


            listConnections.Add(newConnection);
            sem.Release();
        }



        //******************************************************************//
        // Remove a connection to the list of connections in a threadsafe   //
        // way.                                                             //
        //******************************************************************//
        public void RemoveConnection(ThreadConnection connection)
        {
            //*******************************************************//
            // To assist with debugging only wait one second (which  //
            // is a loooong time) and log a console message if timed //
            // out.                                                  //
            //*******************************************************//
            bool timedOut = !sem.WaitOne(1000);
            if (timedOut)
                Console.WriteLine("Wait timed out in connect thread run()");
          

            listConnections.Remove(connection);

            try
            {
                sem.Release();
            }
            catch (Exception e) { }

            Console.Write("Object removed from list of connections.  List now contains ");
            Console.Write(listConnections.Count);
            Console.WriteLine(" connections.");

        }


    }
}

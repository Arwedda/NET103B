using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;


//Joseph Kellaway   10503639
//Zakaria Robinson  10500227

//************************************************************************//
// This project makes an extremely simple traffic light.  Because of the  //
// personal firewall on the lab computers being switched on, this         //
// actually connects to a sort of proxy (running in my office) that       //
// accepts the incomming  connection.                                     //    
// By Nigel.                                                              //
//                                                                        //
// Please use this code, sich as it is,  for any eduactional or non       //
// profit making research porposes on the conditions that.                //
//                                                                        //
// 1.    You may only use it for educational and related research         //
//      pusposes.                                                         //
//                                                                        //
// 2.   You leave my name on it.                                          //
//                                                                        //
// 3.   You correct at least 10% of the typig and spekking mistskes.      //
//                                                                        //
// © Nigel Barlow nigel@soc.plymouth.ac.uk 2016                           //
//************************************************************************//

namespace TrafficLight
{
    public partial class FormTrafficLight : Form
    {
        public FormTrafficLight()
        {
            InitializeComponent();
        }


        //******************************************************//
        // Nigel Networking attributes.                         //
        //******************************************************//
        private int              serverPort       = 5000;
        private int              bufferSize       = 200;
        private TcpClient        socketClient     = null;
        private String           serverName       = "eeyore.fost.plymouth.ac.uk";  //A computer in my office.
//        private String serverName = "192.168.0.5"; //homeIP
        private NetworkStream    connectionStream = null;
        private BinaryReader     inStream         = null;
        private BinaryWriter     outStream        = null;
        private ThreadConnection threadConnection = null;


        //*******************************************************************//
        // This one is needed so that we can post messages back to the form's//
        // thread and don't violate C#'s threading rile that says you can    //
        // only touch the UI components from the form's thread.              //
        //*******************************************************************//
        SynchronizationContext uiContext = null;
    




        //*********************************************************************//
        // Form load.  Display an IP. Or a series of IPs.                      //                               
        //*********************************************************************//
        private void Form1_Load(object sender, EventArgs e)
        {
            //******************************************************************//
            //All this to find out IP number.                                   //
            //******************************************************************//
            IPHostEntry localHostInfo = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

            listBoxOutput.Items.Add("You may have many IP numbers.");
            listBoxOutput.Items.Add("In the Plymouth labs, use the IP that starts 141.163");
            listBoxOutput.Items.Add(" ");


            foreach (IPAddress address in localHostInfo.AddressList)
                listBoxOutput.Items.Add(address.ToString());


            //******************************************************************//
            // Get the SynchronizationContext for the current thread (the form's//
            // thread).                                                         //
            //******************************************************************//
            uiContext = SynchronizationContext.Current;
            if (uiContext == null)
                listBoxOutput.Items.Add("No context for this thread");
            else
                listBoxOutput.Items.Add("We got a context");
 
        }



        //*********************************************************************//
        // Form closing.  If the connection thread was ever created then kill  //
        // it off.                                                             //                               
        //*********************************************************************//
        private void FormTrafficLight_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadConnection != null) threadConnection.StopThread();
        }



        //*********************************************************************//
        // Message was posted back to us.  This is to get over the C# threading//
        // rules whereby we can only touch the UI components from the thread   //
        // that created them, which is the form's main thread.                 // 
        //*********************************************************************//
        public void MessageReceived(Object message)
        {
            String command = (String)message;
            listBoxOutput.Items.Add(command);
            ChangeLights(command);
        }



        //*********************************************************************//
        // Change the status of the lights.                                    //
        //*********************************************************************//
        /// <summary>
        /// Takes in the string command and breaks it apart to be processed.
        /// Then identifies if the light needs an ID and assigns one if it does.
        /// It then identifies if the command contains a colour aswell as it's own ID
        /// and changes accordingly
        /// </summary>
        /// <param name="command">The string sent from the server that contains the commands</param>
        private void ChangeLights(String command)
        {
            if (command == null) return;    // Nothing to do.

            String[] things = command.Split(new char[] { ' ' });

            try
            {
                if (id == 0 && buttonCarArrived.Text == "Car Arrived")
                {
                    id = Convert.ToInt32(things[2]);
                }

                for (int i = 2; i < things.Length; i++) //Loop through the array starting, skipping non-relevant indices. 
                {
                    if (Convert.ToInt32(things[i]) == id && things[i+1].Contains("Red")) // && id matches
                    {
                        labelRed.Visible = true;
                        labelAmber.Visible = false;
                        labelGreen.Visible = false;
                        break;
                    }
                    else if (Convert.ToInt32(things[i]) == id && things[i+1].Contains("Amber"))
                    {
                        labelAmber.Visible = true;
                        labelGreen.Visible = false;
                        break;
                    }
                    else if (Convert.ToInt32(things[i]) == id && things[i+1].Contains("Green"))
                    {
                        labelGreen.Visible = true;
                        labelRed.Visible = false;
                        labelAmber.Visible = false;
                        break;
                    }
                }
            }
            catch (FormatException)
            {
            }
        }



        //*********************************************************************//
        // The OnClick for the "connect"command button.  Create a new client   //
        // socket.   Much of this code is exception processing.                //
        //*********************************************************************//
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                socketClient = new TcpClient(serverName, serverPort);
            }
            catch (Exception ee)
            {
                listBoxOutput.Items.Add("Error in connecting to server");     //Console is a sealed object; we
                listBoxOutput.Items.Add(ee.Message);				 	      //can't make it, we can just access
                labelStatus.Text = "Error " + ee.Message;
                labelStatus.BackColor = Color.Red;
            }

            if (socketClient == null)
            {
                listBoxOutput.Items.Add("Socket not connected");

            }
            else
            {

                //**************************************************//
                // Make some streams.  They have rather more        //
                // capabilities than just a socket.  With this type //
                // of socket, we can't read from it and write to it //
                // directly.                                        //
                //**************************************************//
                connectionStream = socketClient.GetStream();
                inStream  = new BinaryReader(connectionStream);
                outStream = new BinaryWriter(connectionStream);

                listBoxOutput.Items.Add("Socket connected to " + serverName);

                labelStatus.BackColor = Color.Green;
                labelStatus.Text = "Connected to " + serverName;


                //**********************************************************//
                // Discale connect button (we can only connect once) and    //
                // enable other components.                                 //
                //**********************************************************//
                buttonConnect.Enabled    = false;
                buttonCarArrived.Enabled = true;


                //***********************************************************//
                //We have now accepted a connection.                         //
                //                                                           //
                //There are several ways to do this next bit.   Here I make a//
                //network stream and use it to create two other streams, an  //
                //input and an output stream.   Life gets easier at that     //
                //point.                                                     //
                //***********************************************************//
                threadConnection = new ThreadConnection(uiContext, socketClient, this);


                //***********************************************************//
                // Create a new Thread to manage the connection that receives//
                // data.  If you are a Java programmer, this looks like a    //
                // load of hokum cokum..                                     //
                //***********************************************************//
                Thread threadRunner = new Thread(new ThreadStart(threadConnection.Run));
                threadRunner.Start();

                Console.WriteLine("Created new connection class");
            }
        }




        //**********************************************************************//
        // Button cluck for the car arrived button.  All it does is send the    //
        // string "Car" to the server.                                          //
        //**********************************************************************//
        private int id = 0;

        /// <summary>
        /// Button click for the car arrives button. Now identifies if the sender has an
        /// ID, adding new to the string if it doesn't or adding the id to the string if does.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCarArrived_Click(object sender, EventArgs e)
        {
            if (id == 0)
            {
                buttonCarArrived.Text = "Car Arrived";
                SendString("new", textBoxLightIP.Text);
            }
            else
            {
                SendString(id + " Car ", textBoxLightIP.Text);
            }
        }



        //**********************************************************************//
        // Send a string to the IP you give.  The string and IP are bundled up  //
        // into one of there rather quirky Nigel style packets.                 // 
        //                                                                      //
        // This uses the pre-defined stream outStream.  If this strean doesn't  //
        // exist then this method will bomb.                                    //
        //                                                                      //
        // It also does the networking synchronously, in the form's main        //
        // Thread.  This is not good practise; all networking should really be  //
        // asynchronous.                                                        //
        //**********************************************************************//
        private void SendString(String stringToSend, String sendToIP)
        {

            try
            {
                byte[] packet = new byte[bufferSize];
                String[] ipStrings = sendToIP.Split('.'); //Split with . as separator

                packet[0] = Byte.Parse(ipStrings[0]);
                packet[1] = Byte.Parse(ipStrings[1]);   //Think about this.  It assumes the user
                packet[2] = Byte.Parse(ipStrings[2]);   //has entered the IP corrrectly, and 
                packet[3] = Byte.Parse(ipStrings[3]);   //sends the numbers without the bytes.

                int bufferIndex = 4;                    //Start assembling message

                //**************************************************************//
                // Turn the string into an array of characters.                 //
                //**************************************************************//
                int length = stringToSend.Length;
                char[] chars = stringToSend.ToCharArray();


                //**************************************************************//
                // Then turn each character into a byte and copy into my packet.//
                //**************************************************************//
                for (int i = 0; i < length; i++)
                {
                    byte b = (byte)chars[i];
                    packet[bufferIndex] = b;
                    bufferIndex++;
                }

                packet[bufferIndex] = 0;    //End of packet (even though it is always 200 bytes)

                outStream.Write(packet, 0, bufferSize);
                listBoxOutput.Items.Add("Sent " + stringToSend);
            }
            catch (Exception doh)
            {
                listBoxOutput.Items.Add("An error occurred: " + doh.Message);
            }

        }


    }   // End of classy class.
}   // End of namespace

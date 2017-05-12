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
// This project makes an extremely simple server to connect to the other  //
// traffic light clients.  Because of the personal firewall on the lab    //
// computers being switched on, the server cannot use a listening socket  //
// accept incomming connections.  So the server to actually connects to a //
// sort of proxy (running in my office) that accepts the incomming        //
// connection.                                                            //    
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

namespace TrafficLightServer
{

    //New wrapper class.
    public delegate void UI_UpdateHandler(String message);

    public partial class FormServer : Form
    {
        public FormServer()
        {
            InitializeComponent();

            lightList = new List<Light> { };
        }

        //******************************************************//
        // Zak and Joe's variables                              //
        //******************************************************//
        private List<Light> lightList;
        private int lightNumber = 0;
        private Semaphore lightSemj1 = new Semaphore(1, 1); //Semaphore for Junction 1
        private Semaphore lightSemj2 = new Semaphore(1, 1); //Semaphore for Junction 2
        private int j1ID = 0; //ID for Junction 1 
        private int j2ID = 0; //ID for Junction 2

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
        private SynchronizationContext uiContext = null;



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
                inStream         = new BinaryReader(connectionStream);
                outStream        = new BinaryWriter(connectionStream);

                listBoxOutput.Items.Add("Socket connected to " + serverName);

                labelStatus.BackColor = Color.Green;
                labelStatus.Text = "Connected to " + serverName;


                //**********************************************************//
                // Discale connect button (we can only connect once) and    //
                // enable other components.                                 //
                //**********************************************************//
                buttonConnect.Enabled       = false;

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
                Thread threadRunner = new Thread(new ThreadStart(threadConnection.run));
                threadRunner.Start();

                Console.WriteLine("Created new connection class");

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
                int length   = stringToSend.Length;
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
//                listBoxOutput.Items.Add("An error occurred: " + doh.Message); //cross-thread exception error
            }

        }


        //*********************************************************************//
        // Message was posted back to us.  This is to get over the C# threading//
        // rules whereby we can only touch the UI components from the thread   //
        // that created them, which is the form's main thread.                 // 
        //*********************************************************************//
        public void MessageReceived(Object message)
        {
            String toSendIP = textBoxLightIP.Text;
            String command = (String)message;
            listBoxOutput.Items.Add(command);
            CheckMessage(command, toSendIP);
        }

        /// <summary>
        /// Splits the command and identifies which of the keywords it contains.
        /// It then either calls Addcar or Addlight.
        /// </summary>
        /// <param name="command">The command recieved from the other client light</param>
        /// <param name="toSendIP">The IP of the proxy/light client</param>
        public void CheckMessage(String command, String toSendIP)
        {
            String[] things = command.Split(new char[] { ' ' });

            if (command.Contains("new"))
            {
                AddLight(toSendIP);
            }
            else if (command.Contains("Car"))
            {
                AddCar(Convert.ToInt32(things[2]));
            }
        }

        /// <summary>
        /// Adds a new light to the lights list when the client light first sends a message.
        /// </summary>
        /// <param name="toSendIP">The IP for the proxy/light client</param>
        public void AddLight(String toSendIP)
        {
            Light tempLight = new Light();
            tempLight = new Light(lightNumber + 1);

            lightList.Add(tempLight);
            lightNumber += 1;
            SendString(lightNumber + " Red", toSendIP);
        }

        /// <summary>
        /// Creates two new threads to process the two junctions light changes.
        /// It then adds a car to the light that has triggered the addcar message.
        /// It then checks if the car's waiting at that light has reached 10, 
        /// and uses the junction's thread to call the associated lightchange.
        /// The semaphore means that only one light can change at a time at each junction.
        /// The cars are then released to other lights and junctions.
        /// </summary>
        /// <param name="lightID">ID of the light which has triggered the Addcar function</param>
        public void AddCar(int lightID)
        {
            Thread threadJ1 = new Thread(new ThreadStart(LightChangeJ1));
            Thread threadJ2 = new Thread(new ThreadStart(LightChangeJ2));
            int carsThroughLights = 10;

            foreach (Light l in lightList)
            {
                if (l.ID == lightID)
                {
                    l.CarsWaiting += 1;
                    
                    if (l.CarsWaiting >= 10 && l.ID < 5)
                    {
                        lightSemj1.WaitOne();
                        j1ID = l.ID;
                        threadJ1.Start();
                        listBoxOutput.Items.Add("Reached 10 cars");
                        l.CarsWaiting -= carsThroughLights;
                        ReleaseCars(carsThroughLights, l.ID);
                    }
                    else if (l.CarsWaiting >= 10 && l.ID > 4 && l.ID < 9)
                    {
                        lightSemj2.WaitOne();
                        j2ID = l.ID;
                        threadJ2.Start();
                        listBoxOutput.Items.Add("Reached 10 cars");
                        l.CarsWaiting -= carsThroughLights;
                        ReleaseCars(carsThroughLights, l.ID);
                    }
                }
            }
        }

        /// <summary>
        /// Send the cars to another lights based on a random roll. 
        /// </summary>
        /// <param name="cars">The number of cars that pass the lights during
        /// it's green period. although this number is always 10.</param>
        /// <param name="id">The ID of the light that released the cars.</param>
        private void ReleaseCars(int cars, int id)
        {
            Random rnd = new Random();
            int direction = 0;
            for (int i = 0; i <= cars; i++)
            {
                direction = rnd.Next(0, 3);

                switch (id)
                {
                    case 1:
                    case 2:
                    case 3:
                        if (direction == 0)
                        {
                            AddCar(5);
                            listBoxOutput.Items.Add("Car added to junction 2 light 1 from junction 1 light " + id);
                        }
                        break;
                    case 6:
                    case 7:
                    case 8:
                        if (direction == 0)
                        {
                            AddCar(4);
                            listBoxOutput.Items.Add("Car added to junction 1 light 4 from junction 2 light " + (id-4));
                        }
                        break;
                }
            }
        }

        //*********************************************************************//
        // Form closing.  If the connection thread was ever created then kill  //
        // it off.                                                             //                               
        //*********************************************************************//
        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadConnection != null) threadConnection.StopThread();
        }

        /// <summary>
        /// The functions that the threads use to handle colour changes.
        /// </summary>
        private void LightChangeJ1()
        {
            String toSendIP = textBoxLightIP.Text;
                
            SendString(j1ID + " Amber", toSendIP);

            Thread.Sleep(1000);

            SendString(j1ID + " Green", toSendIP);

            Thread.Sleep(4000);
                
            SendString(j1ID + " Red", toSendIP);

            Thread.Sleep(1000);

            lightSemj1.Release();
        }

        /// <summary>
        /// The functions that the threads use to handle colour changes.
        /// </summary>
        private void LightChangeJ2()
        {
            String toSendIP = textBoxLightIP.Text;

            SendString(j2ID + " Amber", toSendIP);

            Thread.Sleep(1000);

            SendString(j2ID + " Green", toSendIP);

            Thread.Sleep(4000);

            SendString(j2ID + " Red", toSendIP);

            Thread.Sleep(1000);

            lightSemj2.Release();
        }
    }   // End of classy class.
}       // End of namespace
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NumGuessFrames1202;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Sockets1_ClientSide_AustinK
{
    public partial class NumGuessSelector : Form
    {
        private Socket connSock = null;         //socket this is connected to the server
        private Thread TRXThread = null;        //thread used to transmit guess and recieve response
        private static BinaryFormatter _bf = new BinaryFormatter(); //bf to serialize data to transmit and deserialize data recieved
        private delegate void delVoidIntResp(int guess, CResponseFrame.ResponseType resp);  //delegate to pass server responses
        ClientSocConnect clientSocConnect = null;   //helper form that is used to establish a connected socket
        private int currentGuess;                   //holds the users current guess value
        public NumGuessSelector()
        {
            InitializeComponent();
        }

        private void NumGuessSelector_Load(object sender, EventArgs e)
        {
            //sets up the form on load
            lowDisplay.Text = guessTrackbar.Minimum.ToString();
            highDisplay.Text = guessTrackbar.Maximum.ToString();           
            guessDisplay.Text = guessTrackbar.Value.ToString();
            ResetForm();
            currentGuess = guessTrackbar.Value;
            sendGuessButton.Enabled = false;
            connectStatusLabel.Text = "Not Connected";
            connectStatusLabel.BackColor = Color.Red;
            connectStatusLabel.ForeColor = Color.White;
        }
        /// <summary>
        /// transmits the users guess to the server 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendGuessButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (connSock != null && connSock.Connected)
                {
                    //send a data request to the server by populating a guess frame
                    CGuessFrame thisGuess = new CGuessFrame(guessTrackbar.Value);

                    //serialize the guess into a data frame
                    MemoryStream ms = new MemoryStream();
                    _bf.Serialize(ms, thisGuess);

                    connSock.Send(ms.GetBuffer(), (int)ms.Length, SocketFlags.None);

                    if (TRXThread == null || !TRXThread.IsAlive)
                    {
                        //recieve the data in a new thread
                        TRXThread = new Thread(TXGuess);
                        TRXThread.IsBackground = true;
                        TRXThread.Start(connSock);
                    }                   
                }
                else
                {
                    Console.WriteLine("You are not connected");
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                connectStatusLabel.BackColor = Color.Red;
                connectStatusLabel.ForeColor = Color.White;
                connectStatusLabel.Text = "Error communicating with server, attempt connection again.";
                ConnectionAttempt();
            }
        }
        /// <summary>
        /// updates the user guess variable based on current trackbar values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void guessTrackbar_Scroll(object sender, EventArgs e)
        {
            guessDisplay.Text = guessTrackbar.Value.ToString();
            currentGuess = guessTrackbar.Value;
        }
        /// <summary>
        /// Creates a helper form that establishes a connection to a socket, and passes the socket back to the main form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            ConnectionAttempt();
        }
        //Attempts to connect the client with a server
        private void ConnectionAttempt()
        {
            sendGuessButton.Enabled = false;
            if (clientSocConnect != null)
                clientSocConnect.Close();

            //creates the form to connect a socket
            clientSocConnect = new ClientSocConnect();
            clientSocConnect.Location = new Point(Right + 50, this.Top); //update soc connectors location
            //shows the modal form and waits for connection
            if (DialogResult.OK == clientSocConnect.ShowDialog())
            {
                AcceptConnection(clientSocConnect.connectedSocket);
                ResetForm();
            }
            else
            {
                if (connSock == null)
                {
                    Invoke(new Action(() => {
                        connectStatusLabel.Text = "Not Connected";
                        connectStatusLabel.BackColor = Color.Red;
                        connectStatusLabel.ForeColor = Color.White;
                        connectButton.Text = "Connect to Play";
                    }));
                }
            }
        }
        #region Socket Functions
        /// <summary>
        /// Function that is used to accept a connected socket to use
        /// </summary>
        /// <param name="cSoc"></param>
        private void AcceptConnection(Socket cSoc)
        {
            if (cSoc.Connected)
            {
                connSock = cSoc;
                //update form variables
                Invoke(new Action(() => { 
                    connectStatusLabel.Text = "Connected";
                    connectStatusLabel.BackColor = Color.Green;
                    connectStatusLabel.ForeColor = Color.White;
                    //connectButton.Text = "Change Connection?";
                    connectButton.Enabled = false;
                    sendGuessButton.Enabled = true;
                }));              
            }     
            else
            {
                //if a connection fails then update the form varibles to indicate that
                Invoke(new Action(() => {
                    connectStatusLabel.Text = " Not Connected";
                    connectStatusLabel.BackColor = Color.Red;
                    connectStatusLabel.ForeColor = Color.White;
                    connectButton.Text = "Connect to Play?";
                    sendGuessButton.Enabled = false;
                }));
            }
        }
        /// <summary>
        /// Thread method that transmits and recieves user guess data and server response data respectively
        /// </summary>
        /// <param name="obj"></param>
        private void TXGuess(object obj)
        {
            //use the connected socket
            Socket soc = (Socket)obj;
            //transient buffer 
            byte[] buff = new byte[2000];
            int iNumRxED = 0; //variable to recieve the buffer data into
            //keep running the thread
            while (true)
            {
                //Check the connection
                try
                {
                    iNumRxED = soc.Receive(buff);   //pull data out of socket buffer

                    if (iNumRxED == 0)  //check for soft disconnect
                    {
                        Console.WriteLine("Soft disconnect");
                        Invoke(new Action(() => {
                            statusLabel.Text = "Disconnected";
                            ConnectionAttempt();
                        }));                       
                        return;
                    }
                }   
                catch (Exception err)   //catches hard disconnects
                {

                    Console.WriteLine("Hard Disconnect Detected: " + err.Message);
                    Invoke(new Action(() => {
                        connectStatusLabel.Text = "Disconnected - Please attempt reconnection...";
                        connectStatusLabel.BackColor = Color.Red;
                        connectStatusLabel.ForeColor = Color.White;
                        ConnectionAttempt();
                    }));
                    return;
                }

                //Connection is good so attempt to deserialize server response
                try
                {
                    MemoryStream ms = new MemoryStream(buff); //memory stream accepts buffer
                    //attempt to get object data from the stream
                    object guessFrame = _bf.Deserialize(ms);

                    if (guessFrame is CResponseFrame rFrame)
                    {
                        Console.WriteLine($"return value is: {rFrame.Response}");
                        //change the limits of the trackbars based on the server response
                        if (InvokeRequired)
                        {
                            Invoke(new delVoidIntResp(ChangeTrackLimits), currentGuess, rFrame.Response);
                        }
                    }
                    else //oops if you see this there was an error
                    {
                        Console.WriteLine($"Frame data error");
                        statusLabel.Text = "Incorrect response data, attempt new connection...";
                        ConnectionAttempt();
                        return;
                    }
                }
                catch (Exception err)   //catches issues if the response is not correctly deserialzed and used
                {
                    Console.WriteLine(err.Message);
                    statusLabel.Text = "Data error, attempt new connection...";
                    ConnectionAttempt();
                    return;
                }
            }
        }
        #endregion

        #region Helper functions
        /// <summary>
        /// updates the trackbar to reflect knowledge gained about the secret number from the server response
        /// </summary>
        /// <param name="guess"></param>
        /// <param name="response"></param>
        private void ChangeTrackLimits(int guess, CResponseFrame.ResponseType response)
        {
            switch (response)
            {
                //guess was too low
                case CResponseFrame.ResponseType.TooLow:
                    guessTrackbar.Minimum = guessTrackbar.Value = currentGuess =  guess + 1;                  
                    lowDisplay.Text = guessTrackbar.Value.ToString();
                    statusLabel.Text = "Your guess was too low";
                    break;
                    //guess was too high
                case CResponseFrame.ResponseType.TooHigh:
                    guessTrackbar.Maximum = guessTrackbar.Value = currentGuess = guess - 1;
                    highDisplay.Text = guessTrackbar.Value.ToString();
                    statusLabel.Text = "Your guess was too high";
                break;
                    //guess was correct, tell the user they one then restart the game
                case CResponseFrame.ResponseType.Correct:
                    CorrectForm cForm = new CorrectForm();
                    cForm.formReset = new delVoidVoid(ResetForm);
                    sendGuessButton.Enabled = false;
                    cForm.Show();
                break;
            }
            guessDisplay.Text = guessTrackbar.Value.ToString();
        }
        /// <summary>
        /// Resets the game form back to starting state
        /// </summary>
        private void ResetForm()
        {
            statusLabel.Text = "Choose a number to guess";
            guessTrackbar.Maximum = 1000;
            guessTrackbar.Minimum = 1;
            guessTrackbar.Value = 500;
            currentGuess = 500;
            highDisplay.Text = "1000";
            lowDisplay.Text = "1";
            guessDisplay.Text = "500";
            sendGuessButton.Enabled = true;
        }

        #endregion
    }
}

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
    //public delegate void delVoidSocket(Socket soc); //delegate to pass a socket
    public delegate void delVoidVoid();             //delegate to perform a void action
    public partial class ClientSocConnect : Form
    {
        private Socket _cSoc = null;            //socket that is to be connected 
        private int port = 1666;                //the prot we expect to use
        private string address = "localhost";   //address to connect to
        //private string address = "bits.net.nait.ca";
        //public delVoidSocket socConnect = null; //delegate to pass out a connected socket **redundant now that form is modal
        //public delVoidVoid formExit;            //void delegate to close the form **redundant now that form is modal
        public Socket Socket    //returns the connected socket
        {
            get { return _cSoc; }
        }
        public ClientSocConnect()
        {
            InitializeComponent();
        }
        public Socket connectedSocket
        {
            get { return _cSoc; }
        }
        /// <summary>
        /// Updatees form info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientSocConnect_Load(object sender, EventArgs e)
        {
           portInput.Text = port.ToString();   //
           addressInput.Text = address;
        }
        /// <summary>
        /// Attempts a connection to the specified location on the defined port number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click(object sender, EventArgs e)
        {
            _cSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);    //initalize the socket

            Console.WriteLine("entered button click");
            try
            {
                //attempt to establish a connection with a server
                Console.WriteLine("looking for connection");
                _cSoc.BeginConnect(
                    addressInput.Text,
                    port,
                    cbConnectDone,
                    _cSoc);
            }
            catch (Exception err)
            {
                Console.WriteLine($"Error connecting to server: {err.Message}");
            }
        }

        #region Socket Functions
        /// <summary>
        /// Async function that completes the socket connection
        /// </summary>
        /// <param name="ar"></param>
        private void cbConnectDone(IAsyncResult ar)
        {
            Socket temp = (Socket)ar.AsyncState;
            try
            {
                Console.WriteLine("Ending previous async request because this should have connected");
                //ends previous async request
                _cSoc.EndConnect(ar);
                //update the form if connection is successful
                if (temp.Connected)
                {
                    DialogResult = DialogResult.OK; //returns the dialog result to the main form



                    //***redudant code now that the form is modal instead of modeless, but leaving here in case I want modal in the future*****
                    //connectIndicator.Invoke(new Action(() =>
                    //{
                    //    Console.WriteLine("Connected now");
                    //    connectIndicator.Text = "Connected";
                    //    connectIndicator.BackColor = Color.Green;
                    //    connectIndicator.ForeColor = Color.White;
                    //}));
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                }
                //else //update the form if connection is unsuccessful
                //{
                //    connectIndicator.Invoke(new Action(() =>
                //    {
                //        connectIndicator.Text = "Unable to connect";
                //        connectIndicator.BackColor = Color.Red;
                //    }));
                //    return;
                //}

                ////attempt to pass the connected socket back to the main form
                //if (socConnect != null)
                //    socConnect.Invoke(temp);
                //*****************************************************************************************************************************
                    
            }
            catch (Exception err)
            {

                Console.WriteLine(err.Message);
            }
        }
        #endregion

        #region Helper Functions

        #endregion
        ///***Reduedant form closing method now that the form is modal***
        //private void ClientSocConnect_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    if (formExit != null)
        //    {
        //        formExit();
        //    }
        //}
    }
}

//Author: Austin Klevgaard
// 1202_CMPE2800_OTH_NGS
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

namespace Sockets1_ServerSide_AustinK
{
    public partial class ServerForm : Form
    {
        List<ClientConnHandle> _connList = new List<ClientConnHandle>();    //list to hold connected client sockets
        private Socket _listenSoc = null;                                   //socket used as a listener to look for clients
        private delegate void delVoidCCH(ClientConnHandle newCon);          //delegate used to pass on Client handler classes
        public ServerForm()
        {
            InitializeComponent();
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            listenerStatus.Text = "Listener Status: ";
            StartListenerSocket();
        }
        /// <summary>
        /// Timer polls the server form to see if the listener should be turned on or off based on current client connections
        /// Timer will also remove any dead client connections from the connections list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listenerPollTimer_Tick(object sender, EventArgs e)
        {
            if (_listenSoc == null && _connList.Count < (int)maxClientsUD.Value)
            {
                StartListenerSocket();
            }
            if (_listenSoc != null && _connList.Count >= (int)maxClientsUD.Value)
            {
                StopListernerSocket();
            }
            //removes all disconnected sockets
            _connList.RemoveAll(soc => !soc.IsConnected);
            //updated client number info on server form
            currentClientCountText.Text = _connList.Count.ToString();
                    
        }
        #region Socket Functions
        /// <summary>
        /// Function that accepts an IAsyncResult from a listener socket connection and 
        /// create a new client socket handler to recieve and transmit data to the client
        /// </summary>
        /// <param name="ar"></param>
        private void cbAccept(IAsyncResult ar)
        {
            try
            {
                if ( _connList.Count < (int)maxClientsUD.Value) //number of clients limited 
                {
                    try
                    {
                        //create the client handler, pass it the connected socket, then add the new client handler to the 
                        //main form list.
                        ClientConnHandle newConnect = new ClientConnHandle(_listenSoc.EndAccept(ar));
                        Invoke(new delVoidCCH(AddNewConnection), newConnect);
                    }
                    catch (Exception err)
                    {

                        Console.WriteLine(err.Message);
                        return;
                    }
                    //update the form text to indicate a new connection has been established                
                    Invoke(new Action(() => {
                        currentClientCountText.Text = _connList.Count().ToString();
                    }));
                    try
                    {
                        //if socket pass off was successful then start listening again
                        //start waiting for a connection
                        _listenSoc.BeginAccept(cbAccept, null);

                    }
                    catch (Exception err)   //throw an error if the listener cannot start again
                    {

                        Console.WriteLine(err.Message);
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return;
            }
        }
        /// <summary>
        /// Adds a new connected socket to the list of connected sockets
        /// </summary>
        /// <param name="newClient"></param>
        private void AddNewConnection(ClientConnHandle newClient)
        {
            _connList.Add(newClient);
        }
        /// <summary>
        /// Starts the listener socket to wait to recieve a client request for connection
        /// </summary>
        private void StartListenerSocket()
        {
            //intitalize the listener
            _listenSoc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //binds the socket to the endpoint
                _listenSoc.Bind(new IPEndPoint(IPAddress.Any, 1666));
            }
            catch (Exception err)
            {

                Console.WriteLine(err.Message);
                return;
            }
            try
            {
                _listenSoc.Listen(5);   //start the socket listening with a backlog of 5 client requests
            }
            catch (Exception err)
            {

                Console.WriteLine(err.Message);
                return;
            }
            try
            {
                //start waiting for a connection
                _listenSoc.BeginAccept(cbAccept, null);
                //update the server form to show that the listener is on
                Invoke(new Action(() => {
                    listenerStatusDisplay.Text = "On";
                }));
            }
            catch (Exception err)
            {

                Console.WriteLine(err.Message);
                return;
            }
        }
        /// <summary>
        /// Stops the listener if it is not ready to accept any more clients at this time
        /// </summary>
        private void StopListernerSocket()
        {
            try
            {
                //**how can I do this better? I feel like this is just a rough disconnect?
               // _listenSoc.Shutdown(SocketShutdown.Both); 

                _listenSoc.Close();
                _listenSoc = null;  //listener is a null reference
                //update status to show that the listener is off
                Invoke(new Action(() => {
                    listenerStatusDisplay.Text = "Off";
                }));
            }
            catch (Exception err)
            {

                Console.WriteLine(err.Message);
            }

        }
        #endregion

        #region Helper Functions

        #endregion


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumGuessFrames1202;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace Sockets1_ServerSide_AustinK
{
    class ClientConnHandle
    {
        private Socket _connSoc = null;             //the server socket that is connected to the client
        private Thread _commThread = null;          //background thread to run data tx and rx in
        private int _secretNum;                     //int to hold the secred number
        private bool threadRun = true;              //currently redundant bool that would allows the main thread to stop the background thread
        private BinaryFormatter _bf = new BinaryFormatter();    //binary formatter to serialize data
        private static Random _rand = new Random();             //rng

        public bool IsConnected     //public bool property to poll to see if this client handler has been disconnected
        {
            get { return _connSoc.Connected; }
        }
        /// <summary>
        /// Upon construction a connected socket is passed into this class, and a secret number to guess is generated
        /// Then a background thread is started to send and recieve data in regards to the number guessing game
        /// </summary>
        /// <param name="_newConnSoc"></param>
        public ClientConnHandle(Socket _newConnSoc)
        {
            _connSoc = _newConnSoc;
            _secretNum = GetSecretNum();

            //recieve the data in a new thread
            _commThread= new Thread(CommunicateThreadBody);
            _commThread.IsBackground = true;
            _commThread.Start(_connSoc);
        }
        //Handling methods

        /// <summary>
        /// Waits for a user guess to be transmitted through the socket, which is then compared to the secret number.
        /// A response is created based on the users guess compared to the secret number, then the response is sent back to the client
        /// </summary>
        /// <param name="obj"></param>
        private void CommunicateThreadBody(Object obj)
        {
            Socket soc = (Socket)obj;   //accept connected socket
            //transient buffer 
            byte[] buff = new byte[2000];
            while (threadRun)
            {
                try
                {
                    int playerGuess = soc.Receive(buff);    //recieve the player guess as a serialized array of bytes
                    if (playerGuess == 0)
                    {
                        Console.WriteLine("Soft disconnect");
                        return;
                    }
                    MemoryStream ms = new MemoryStream(buff); //memory stream accepts buffer
                    try
                    {
                        object guessFrame = _bf.Deserialize(ms);    //deserialize the player guess
                        if (guessFrame is CGuessFrame guess)
                        {
                            CResponseFrame response = null; //provide an appropriate response to the player guess
                            if (guess.Guess < _secretNum)
                                response = new CResponseFrame(CResponseFrame.ResponseType.TooLow);
                            if (guess.Guess > _secretNum)
                                response = new CResponseFrame(CResponseFrame.ResponseType.TooHigh);
                            if (guess.Guess == _secretNum)  //if the guess is correct get a new secret number
                            {
                                response = new CResponseFrame(CResponseFrame.ResponseType.Correct);
                                _secretNum = GetSecretNum();
                            }

                            //get a fresh memory stream to send data to the client
                            MemoryStream msOut = new MemoryStream();
                            _bf.Serialize(msOut, response);
                            //send the data out to the client
                            soc.Send(msOut.GetBuffer(), (int)msOut.Length, SocketFlags.None);
                        }
                    }
                    catch (Exception err)
                    {

                        Console.WriteLine(err.Message);
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("Hard Disconnect Detected: " + err.Message);
                    return;
                }
            }
        }
        /// <summary>
        /// Provides a secret number between 1 and 1000 inclusive
        /// </summary>
        /// <returns></returns>
        private int GetSecretNum()
        {
            return _rand.Next(1, 1001);
        }

    }
}

using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System;

// Use this class to send requests to the server.
public class Request : RunAbleThread
{

    private byte[] audio_stream;
    private string user_input;
    private string username;

    // Constructors based on differnt requirements for the request. 
    public Request(string user_input, string username, byte[] audio_stream)
    {
        this.audio_stream = audio_stream;
        this.user_input = user_input;
        this.username = username;
    }

    public Request(string user_input, byte[] audio_stream)
    {      
        this.audio_stream = audio_stream;
        this.user_input = user_input;
    }
    public Request(string user_input)
    {
        this.user_input = user_input;
    }

    public Request(string user_input, string username)
    {
        this.user_input = user_input;
        this.username = username;

    }


    //Defines the returned message from the server.
    public string ServerMessage { get; set; }

    protected override void Run()
    {
        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (RequestSocket client = new RequestSocket())
        {
            // Connect to VM 
            client.Connect("tcp://192.168.0.111:5555");

            //Local machine
            //client.Connect("tcp://localhost:5555");

            Debug.Log($"Sending value: {user_input}");
            //Create new message object to populate.
            var message = new NetMQMessage();

            // User input must be passed in as a requirement, so the server knows what to do.
            message.Append(user_input);

            //If we have a username or audio-stream, pass those in too.
            if (username != null) message.Append(username);
            if (audio_stream != null) message.Append(audio_stream);



            // ref https://stackoverflow.com/questions/40146582/netmq-client-to-client-messaging
            //Try to send a message.
            if (client.TrySendMultipartMessage(message))
            {
                string returnMessage = null;
                bool gotMessage = false;
                // Wait until a message has been returned.
                while (Running)
                {
                    //Receive returned message.
                    gotMessage = client.TryReceiveFrameString(out returnMessage); // this returns true if it's successful
                    if (gotMessage) break;
                }
                //If we have the message, 
                if (gotMessage)
                {
                    // Populate public var so it can be accessed. 
                    ServerMessage = returnMessage;
                    Debug.Log("Received -------------- " + ServerMessage);

                }

            }
            else
            {
                //We have no connection.
                ServerMessage = "No connection";
            }

            

        }

        //Required otherwise Unity will freeze.
        NetMQConfig.Cleanup(); 
        Debug.Log("Received -------------- " + ServerMessage);
    }

}
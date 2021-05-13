import time
import zmq
from datetime import datetime
import extract_features
import test_model
import user_setup
import pickle

"""
This file is the python server, run this to begin listening for requests sent from NetMQ C# client.
"""

# Used to keep track of how many training files have been written.
sample_counter = 0


def parse_input(input, streamOrName=None, stream=None):
    """ Parse the input from the incoming request. Optionally give the input stream with username."""

    global sample_counter
    # If the user hasn't been setup yet, create and train a new model. It will return True if successful, False if not.
    if (input == b"1"):
        print("Begining user Setup")
        name = streamOrName.decode()
        if user_setup.confirm_user(name) == "False":
            sample_counter = user_setup.record_training_data_stream(
                name, stream, sample_counter)
            return f"File{sample_counter} Saved"

    # Main function to detect if the user is speaking or not. Create MFCC from audio data then test it against the models. Returns  detected name.
    elif (input == b"2"):
        combind = extract_features.incoming_live_mfcc(streamOrName)
        return test_model.testStream(combind, streamOrName)
    # Get current users
    elif (input == b"3"):
        print("Getting user information")
        users = user_setup.get_users()
        return users
    # Create new GMM model
    elif (input == b"4"):
        name = streamOrName.decode()
        if user_setup.confirm_user(name) == "False":
            sample_counter = 0
            print("Creating GMM model")
            complete = user_setup.create_GMM_model()
            trainedfilelist = open("training_set_addition.txt", 'w')
            trainedfilelist.truncate(0)
            return complete

    # Delete user
    elif (input == b"5"):

        name = streamOrName.decode()
        return user_setup.remove_user(name)

    # Return if abnormal input is given.
    return "No input found"


if __name__ == "__main__":
    print("------ Opening sockets - -----")
    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind("tcp://*:5555")

    print("------ Starting server, sever is now listening for requests. ------\n")
    while True:
        #  Wait for next request from client
        now = datetime.now()
        dt_string = now.strftime("%H:%M:%S:%f %d/%m/%Y")

        # Receive and unpack the multipart message sent from the server. It should be two pieces, one for the menu and one for the audio stream.
        message = socket.recv_multipart(copy=True)
        lenMessage = len(message)
        print("------------- New Request -------------")
        print("Received request: %s, msg-size: %s AT %s \n" %
              (message[0], len(message), dt_string))

        # Parse the messages received from the server, depending if only an input has been sent or if an audio-stream has also been sent.
        # Return value will be the detected voice or True / False based on input context.
        if lenMessage == 3:
            returned_value = parse_input(message[0], message[1], message[2])
        elif lenMessage == 2:
            returned_value = parse_input(message[0], message[1])
        else:
            print("Single value")
            returned_value = parse_input(message[0])
        print("Returned value: %s" % (str(returned_value)))
        print("---------------------------------------\n")

        train_file = "./model-classificaiton.txt"
        trainedfilelist = open(train_file, 'a')
        trainedfilelist.write(returned_value + "," + dt_string + "\n")
        trainedfilelist.close()
        #  Send reply back to client
        # socket.send(returned_value)
        socket.send_string(returned_value)

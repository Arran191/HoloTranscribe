import extract_features
import test_model
import os
import pickle
import user_setup
import time

"""
This file is used to mimic data coming from the Unity application, each choice
would be replicated in server.py, this would be sent from the C# client.
"""

if __name__ == "__main__":
    while True:
        choice = int(input(
            "\n 1. Train new Model \n 2.Test Model\n3.  "))
        if(choice == 1):
            name = (input("Please Enter Your user name:"))
            print("You will be asked to speak five times")
            time.sleep(1.0)
            user_setup.record_training_data(name)
            print("Training GMM model")
            time.sleep(0.5)
            user_setup.create_GMM_model(name)
        elif (choice == 2):
            # Need to keep running it for some reason until it starts to pick up
            # NoSound then it seems to be working fine.
            name = (input("Please Enter Your user name:"))
            print("Use CTRL C to cancel ")
            time.sleep(1.0)
            extract_features.live_mfcc(name)
        if(choice > 4):
            exit()

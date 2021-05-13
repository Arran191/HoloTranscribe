import os
from os import listdir
import wave
import time
import pickle
# Install through Conda is there is a problem
import pyaudio
import warnings
import numpy as np
from sklearn import preprocessing
from scipy.io.wavfile import read
import python_speech_features as mfcc
from sklearn.mixture import GaussianMixture
import io
import extract_features
import struct
import random
warnings.filterwarnings("ignore")


def record_training_data(name):
    """ Used to record training data. Pass in the name of the user and it will record and save five files of the user speaking. This is classed as the GMM training set."""

    # Create 5 data sets of the user
    for count in range(5):
        FORMAT = pyaudio.paInt16
        CHANNELS = 1
        RATE = 16000
        CHUNK = 4000
        RECORD_SECONDS = 10
        device_index = 2
        audio = pyaudio.PyAudio()
        stream = audio.open(format=FORMAT, channels=CHANNELS,
                            rate=RATE, input=True, input_device_index=1,
                            frames_per_buffer=CHUNK)

        print("Recording started: " + str(count))
        Recordframes = []

        # Record set amount of training data to be used later.
        for i in range(0, int(RATE / CHUNK * RECORD_SECONDS)):
            data = stream.read(CHUNK)
            Recordframes.append(data)
        print("Recording stopped")

        # End the recording stream
        stream.stop_stream()
        stream.close()
        audio.terminate()

        # Save the file
        OUTPUT_FILENAME = name+"-sample"+str(count)+".wav"
        WAVE_OUTPUT_FILENAME = os.path.join("training_set", OUTPUT_FILENAME)
        trainedfilelist = open("training_set_addition.txt", 'a')
        trainedfilelist.write(OUTPUT_FILENAME+"\n")
        waveFile = wave.open(WAVE_OUTPUT_FILENAME, 'wb')
        waveFile.setnchannels(CHANNELS)
        waveFile.setsampwidth(audio.get_sample_size(FORMAT))
        waveFile.setframerate(RATE)
        waveFile.writeframes(b''.join(Recordframes))
        waveFile.close()
        print("File saved")
    print("Training samples taken.")


def remove_user(username):
    """
        Removes user's file if required. Returns if the file has been removed or not.    
    """
    model_path = "./trained_models/"
    filepath = model_path + username + ".gmm"

    if os.path.isfile(filepath):
        print(f"Found {username} - Deleting profile")
        os.remove(filepath)
        return "File Removed"
    else:
        return "File not found"


def record_training_data_stream(name, stream, sampleCounter):
    """ Save the wav file from the server, accepts username, byte-stream and sampleCounter which is to stop data being written over."""

    # Stops data from being written over
    sampleCounter = sampleCounter + 1

    OUTPUT_FILENAME = name+"-sample-"+str(sampleCounter)+".wav"
    WAVE_OUTPUT_FILENAME = os.path.join("training_set", OUTPUT_FILENAME)
    trainedfilelist = open("training_set_addition.txt", 'a')
    trainedfilelist.write(OUTPUT_FILENAME + "\n")

    # Write to file.
    f = open(WAVE_OUTPUT_FILENAME, 'wb')
    f.write(stream)
    f.close()
    print(f"File: {OUTPUT_FILENAME} saved")
    return sampleCounter


def create_GMM_model():
    """ After creating the training data, use this methord to train the GMM algorithm. It will create a separate model for each user voice. """
    source = "./training_set/"
    dest = "./trained_models/"
    train_file = "./training_set_addition.txt"
    file_paths = open(train_file, 'r')
    count = 1
    features = np.asarray(())
    for path in file_paths:
        path = path.strip()
        print(f"{count} - {path}")

        sr, audio = read(source + path)
        # Extract the features to be trained on from the file
        vector = extract_features.extract_features_all(audio, sr)

        if features.size == 0:
            features = vector
        else:
            features = np.vstack((features, vector))

        # One all features have been extracted
        if count == 5:
            # Create and train model based on the features extracted
            gmm = GaussianMixture(n_components=2, max_iter=700,
                                  covariance_type='full', n_init=10)
            # Fit the data
            gmm.fit(features)

            # Dump the trained gaussian model into files
            picklefile = path.split("-")[0]+".gmm"
            pickle.dump(gmm, open(dest + picklefile, 'wb'))
            print('+ modeling completed for speaker:', picklefile,
                  " with data point = ", features.shape)
            features = np.asarray(())
            count = 0
        count = count + 1
    return "True"


def confirm_user(name):
    """ Confirms if the user already has a trained model or not.  """

    # Get user file names and convert them into a single list
    model_path = "./trained_models"
    file_paths = onlyfiles = [f for f in listdir(model_path)]
    paths = [(path.split("-")[0]) for path in file_paths]
    paths = list(dict.fromkeys(paths))

    # If the name is found then...
    if any(name in s for s in paths):
        # name found
        print("File found: %s" % (name))
        return "True"
    else:
        # name noy found
        return "False"


def get_users():
    """
        Get the user-names from the GMM files.
    """
    model_path = "./trained_models"
    file_paths = onlyfiles = [f for f in listdir(model_path)]
    paths = [(path.split(".gmm")[0]) for path in file_paths]
    paths = list(dict.fromkeys(paths))
    delim = ","
    temp = list(map(str, paths))
    return delim.join(temp)

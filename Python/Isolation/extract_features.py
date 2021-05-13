import numpy as np
from sklearn import preprocessing
import python_speech_features as mfcc
import struct
import time
import pyaudio
import numpy as np
import test_model


def calculate_delta(array):
    """Calculate and returns the delta of given feature vector matrix"""

    rows, cols = array.shape
    deltas = np.zeros((rows, 20))
    N = 2
    for i in range(rows):
        index = []
        j = 1
        while j <= N:
            if i-j < 0:
                first = 0
            else:
                first = i-j
            if i+j > rows-1:
                second = rows-1
            else:
                second = i+j
            index.append((second, first))
            j += 1
        deltas[i] = (array[index[0][0]]-array[index[0][1]] +
                     (2 * (array[index[1][0]]-array[index[1][1]]))) / 10
    return deltas


def extract_features_all(audio, rate):
    """Extract MFCC feature from an audio stream, dynamically add in the rate also. 
        MFCC is created in this function.
     """

    mfcc_feature = mfcc.mfcc(audio, rate, 0.025, 0.01,
                             20, nfft=1200, appendEnergy=True)
    mfcc_feature = preprocessing.scale(mfcc_feature)
    delta = calculate_delta(mfcc_feature)
    combined = np.hstack((mfcc_feature, delta))
    return combined


def extract_features(mfcc_feature):
    """Extract 20 dim mfcc features from an audio, performs CMS and combines
    delta to make it 40 dim feature vector"""
    delta = calculate_delta(mfcc_feature)
    combined = np.hstack((mfcc_feature, delta))
    return combined


def live_mfcc(name):
    """Take in pyaudio stream and detect incoming speaker - Used to mimic a request but 
        is continuos.  """

    # ref : https://gist.github.com/sshh12/62c740b329229c7292f2a7b520b0b6f3
    rate = 16000
    chunk_size = rate // 4

    p = pyaudio.PyAudio()
    stream = p.open(format=pyaudio.paFloat32,
                    channels=1,
                    rate=rate,
                    input=True,
                    input_device_index=1,
                    frames_per_buffer=chunk_size)

    frames = []
    while True:
        # Does go a bit fast which ends up looking like it is detecting the wrong voice, but it isn't.
        # Read Datastream and return if
        print("------------- New Request -------------")
        data = stream.read(chunk_size)
        combind = incoming_live_mfcc(data)
        winner = test_model.testStream(combind, name)
        print("Winner %s " % (winner))
        print("---------------------------------------\n")


def incoming_live_mfcc(stream):
    """Take in byte audio stream and try to detect voice """
    rate = 16000
    chunk_size = rate // 4

    frames = []

    while True:
        start = time.time()
        data = stream
        frames.append(data)
        data = np.fromstring(data, dtype=np.float32)
        melspec = mfcc.mfcc(data, rate, 0.025, 0.01,
                            20, nfft=1200, appendEnergy=True)

        melspec = preprocessing.scale(melspec)

        return extract_features(melspec)
        #test_model.testStream(combind, "Testuser")

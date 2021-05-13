import time
import os
import pickle
import numpy as np
from scipy.io.wavfile import read
import extract_features
import warnings
warnings.filterwarnings("ignore")


def testStream(vector, name):
    """ Used to test an incoming data stream to detect who the closest winner and therefore the detected voice. 
    Vector should be a MFCC feature and Delta.. """

    start = time.time()
    modelpath = "./trained_models"

    # Get GMM files
    gmm_files = [os.path.join(modelpath, fname) for fname in
                 os.listdir(modelpath) if fname.endswith('.gmm')]

    if gmm_files == None:
        return "No speakers"

    # Load the Gaussian gender Models
    models = [pickle.load(open(fname, 'rb')) for fname in gmm_files]
    speakers = [fname.split("\\")[-1].split(".gmm")[0] for fname
                in gmm_files]

    print(speakers)
    log_likelihood = np.zeros(len(models))

    # Score the comparison models
    for i in range(len(models)):
        gmm = models[i]  # checking with each model one by one
        scores = np.array(gmm.score(vector))
        log_likelihood[i] = scores.sum()

    #print(f"LogLikelyhood LEN{len(log_likelihood)}")
    #print(f"Log Likelyhood: {log_likelihood}")

    # Highest log-likelihood is the winner.
    winner = np.argmax(log_likelihood)

    print("detected as - ", speakers[winner])

    end = time.time()
    print("Elapsed Time: " + str(end - start))

    #Train_file = "./model-classificaiton.txt"
    #trainedfilelist = open(train_file, 'a')
    #trainedfilelist.write(speakers[winner] + "," + str(end - start) + "\n")
    # trainedfilelist.close()

    return speakers[winner]

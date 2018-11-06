#!/usr/bin/python

import unittest
from subprocess import call, Popen, DEVNULL, check_output
import json
import requests
import time

import compile

class TestPASS(unittest.TestCase):

    self.pravda = None

    # Set up Pravda once before running the TestCase
    def setUpClass(self):
        # Compile main & test contracts
        compile.compile_contracts()

        # Delete current pravda blockchain data
        call(["rm", "-rf", "pravda-data"])
        # Init new local pravda blockchain
        call(["pravda", "node", "init", "--local", "--coin-distribution", "test-coin-dist.json"])

        # Run Pravda node in a new subprocess
        print("Starting pravda node")
        self.pravda = Popen(["pravda", "node", "run"], stderr=DEVNULL, stdout=DEVNULL)
        # Wait for it to load
        time.sleep(10)

        # Check if Pravda lauched
        def check_pravda_status():
            try:
                r = requests.get("http://localhost:8080/ui")
                return r.status_code
            except:
                time.sleep(3)
                return check_pravda_status()
        print("Pravda status: {}".format(str(check_pravda_status())))

    # Set up new PASS contract for each test
    def setUp(self):
        # Deploy smart-contract to Pravda
        res = check_output(["pravda", "broadcast", "deploy", "--wallet", "test-wallet.json", "--input", "PASS.pravda"])
        address = json.loads(res)["result"]["effects"][0]["address"]
        print("Deployed contract address: {}".format(address))

    def test_emit(self):
        pass

    # Terminate Pravda after testing
    def tearDownClass(self):
        print('Terminating pravda')
        self.pravda.kill()

if __name__ == '__main__':
    unittest.main()

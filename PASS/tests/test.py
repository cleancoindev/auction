#!/usr/bin/python

import unittest
from subprocess import call, Popen, DEVNULL, check_output
import signal
import json
import requests
import time

import compile

class TestPASS(unittest.TestCase):

    # Pravda instance
    pravda = None
    # Result of setUp work
    res = None

    # Set up Pravda once before running the TestCase
    @classmethod
    def setUpClass(self):
        # Compile main & test contracts
        compile.compile_contracts(['emit', 'transfer'])

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

        # Deploy smart-contract to Pravda
        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/test-wallet.json", "-l", "9000000",
                            "-i", "PASS.pravda", "--program-wallet", "wallets/program-wallet.json"])

        print("Deployed PASS to Pravda")

    # Set up particular contract test
    def setUp(self):
        # Get the name of the test
        name = self._testMethodName[5:]
        # Generate program wallet for the test
        call(["pravda", "gen", "address", "-o", "wallets/{}-test-wallet.json".format(name)])

        # Deploy the tester program
        address = json.loads(check_output(
            ["pravda", "broadcast", "deploy", "-w", "wallets/test-wallet.json", "-l", "9000000",
             "-i", "pcalls/{}.pravda".format(name), "--program-wallet",
             "wallets/{}-test-wallet.json".format(name)]))['effects'][0]['address']

        print("{}.pravda deployed on {}".format(name, address))

        # Run ASM code calling test program and save output to res
        res = str(check_output(
            'echo "push \\"test_{}\\" push x{} push 1 pcall"'.format(name, address) +
            '| pravda compile asm |'+
            'pravda broadcast run -w wallets/program-wallet.json -l 9000000 '+
            '--program-wallet wallets/{}-test-wallet.json'.format(name), shell=True)) \
            .replace('\\n', '')[2:-1]
        self.res = json.loads(res)["executionResult"]["success"]["stack"]

        # Clean up the program wallet
        call(["rm", "-rf", "wallets/{}-test-wallet.json".format(name)])

    # Test if assets can be emitted
    def test_emit(self):
        self.assertEqual(['int32.1'], self.res)

    # Test if assets can be transfered
    def test_transfer(self):
        self.assertEqual(['int32.1'], self.res)

    @classmethod
    def tearDownClass(self):
        # Terminate Pravda after testing
        print('Terminating pravda')
        self.pravda.send_signal(signal.SIGINT)
        self.pravda.wait()

if __name__ == '__main__':
    unittest.main()

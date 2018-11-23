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
        compile.compile_contracts(['emit', 'transfer', 'itemlist'])

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

        time.sleep(3)

        # Deploy smart-contract to Pravda
        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/test-wallet.json", "-l", "9000000",
                            "-i", "PASS.pravda", "--program-wallet", "wallets/program-wallet.json"])
        print("PASS.pravda deployed on fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b")

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
        res = check_output(
            'echo "push \\"test_{}\\" push x{} push 1 pcall"'.format(name, address) +
            '| pravda compile asm |'+
            'pravda broadcast run -w wallets/program-wallet.json -l 9000000 '+
            '--program-wallet wallets/{}-test-wallet.json'.format(name), shell=True)
        self.res = json.loads(res.decode('utf-8-sig'))["executionResult"]["success"]

        # Clean up the program wallet
        call(["rm", "-rf", "wallets/{}-test-wallet.json".format(name)])

    # Test if assets can be emitted
    def test_emit(self):
        self.assertEqual(self.res["stack"][0], 'utf8.{'+
            '"owner": "E04919086E3FEE6F1D8F6247A2C0B38F874AB40A50AD2C62775FB09BAA05E342",'+
            '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",'+
            '"metaId": "https://some_url/0000000000000000000000000000000000000000000000000000000000000002"'+
        '}')

    # Test if assets can be transfered
    def test_transfer(self):
        self.assertEqual(self.res["stack"][0], 'utf8.{'+
            '"owner": "0000000000000000000000000000000000000000000000000000000000000000",'+
            '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",'+
            '"metaId": "https://some_url/0000000000000000000000000000000000000000000000000000000000000002"'+
        '}')

    # Test if item list is working
    def test_itemlist(self):
        self.assertEqual(len(self.res["stack"]), 0)

    @classmethod
    def tearDownClass(self):
        # Terminate Pravda after testing
        print('Terminating pravda')
        time.sleep(2)
        self.pravda.send_signal(signal.SIGINT)
        self.pravda.wait()

if __name__ == '__main__':
    unittest.main()

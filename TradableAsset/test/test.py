#!/usr/bin/python

import unittest
from subprocess import call, Popen, DEVNULL, check_output
import signal
import json
import requests
import time

TA_path = "../source/GT/bin/TradableGTAsset.pravda"
pcall_file_path = "pcalls/{0}/bin/{0}.pravda"

class TestTradableAsset(unittest.TestCase):

    # Pravda instance
    pravda = None
    # Result of setUp work
    res = None

    test_calls = ['Emit', 'Transfer', 'Itemlist', 'UsersItems']

    # Set up Pravda once before running the TestCase
    @classmethod
    def setUpClass(self):
        # Compile main & test contracts
        output = check_output(["dotnet", "publish", "../source/TradableAsset.sln"])
        print("Programs compiled")

        # Delete current pravda blockchain data
        call(["rm", "-rf", "pravda-data"])
        # Init new local pravda blockchain
        call(["pravda", "node", "init", "--local", "--coin-distribution", "test-coin-dist.json"])
        print("New pravda node initialized")

        # Run Pravda node in a new subprocess
        print("Starting pravda node")
        self.pravda = Popen(["pravda", "node", "run"], stderr=DEVNULL, stdout=DEVNULL)
        # Wait for it to load
        time.sleep(15)

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
                            "-i", TA_path, "--program-wallet", "wallets/program-wallet.json"])
        print("TradableGTAsset.pravda deployed on fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b")

    # Set up particular contract test
    def setUp(self):
        # Get the name of the test
        name = self._testMethodName[5:]
        # Generate program wallet for the test
        call(["pravda", "gen", "address", "-o", "wallets/{}-test-wallet.json".format(name)])

        # Deploy the tester program
        address = json.loads(check_output(
            ["pravda", "broadcast", "deploy", "-w", "wallets/test-wallet.json", "-l", "9000000",
             "-i", pcall_file_path.format(name), "--program-wallet",
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
    def test_Emit(self):
        self.assertEqual(self.res["stack"][0], 'utf8.{'+
            '"id": "1",'+
            '"owner": "e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342",'+
            '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",'+
            '"metaId": "https://some_url/0000000000000000000000000000000000000000000000000000000000000002"'+
        '}')

    # Test if assets can be transfered
    def test_Transfer(self):
        self.assertEqual(self.res["stack"][0], 'utf8.{'+
            '"id": "4",'+
            '"owner": "0000000000000000000000000000000000000000000000000000000000000000",'+
            '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",'+
            '"metaId": "https://some_url/0000000000000000000000000000000000000000000000000000000000000002"'+
        '}')

    # Test if item list is working
    def test_Itemlist(self):
        self.assertEqual(len(self.res["stack"]), 0)

    # Test if getting all user items works
    def test_UsersItems(self):
        self.assertEqual(self.res["stack"][0], 'utf8.['+
            '{' +
                '"id": "5",'+
                '"owner": "a1fe824f193bcee32f33b9e01245bd41f05a157eca73daf65d70ebd27430836d",' +
                '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",' +
                '"metaId": "https://some_url/0000000000000000000000000000000000000000000000000000000000000001"' +
            '},' +
            '{' +
                '"id": "6",'+
                '"owner": "a1fe824f193bcee32f33b9e01245bd41f05a157eca73daf65d70ebd27430836d",' +
                '"externalId": "0000000000000000000000000000000000000000000000000000000000000002",' +
                '"metaId": "https://some_url/0000000000000000000000000000000000000000000000000000000000000002"' +
            '},' +
            '{' +
                '"id": "7",'+
                '"owner": "a1fe824f193bcee32f33b9e01245bd41f05a157eca73daf65d70ebd27430836d",' +
                '"externalId": "0000000000000000000000000000000000000000000000000000000000000003",' +
                '"metaId": "https://some_url/0000000000000000000000000000000000000000000000000000000000000003"' +
            '}' +
        ']')

    @classmethod
    def tearDownClass(self):
        # Terminate Pravda after testing
        print('Terminating pravda')
        time.sleep(2)
        self.pravda.send_signal(signal.SIGINT)
        self.pravda.wait()
        print('Cleaning up the directory')
        call(["rm", "-rf", "pravda-data"])
        call(["rm", "-rf", TA_path])
        for test_call in self.test_calls:
            call(["rm", "-rf", pcall_file_path.format(test_call)])

if __name__ == '__main__':
    unittest.main()

#!/usr/bin/python

import unittest
from subprocess import call, Popen, DEVNULL, check_output
import signal
import json
import requests
import time

TA_path = "../../TradableAsset/source/bin/TradableAsset.pravda"
auction_path = "../source/bin/Auction.pravda"
pcall_file_path = "pcalls/{0}/bin/{0}.pravda"

class TestTradableAsset(unittest.TestCase):

    maxDiff = None
    # Pravda instance
    pravda = None
    # Result of setUp work
    res = None
    
    test_calls = ['SetUpTradableAsset', 'SetUpAuction', 'NewLot', 'Buy', 'CloseLot']

    # Set up Pravda once before running the TestCase
    @classmethod
    def setUpClass(self):
        # Compile main & test contracts
        output = check_output(["dotnet", "publish", "../source/Auction.sln"])
        print("Programs compiled")

        
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
        
        print("Deploying programs")

        # Deploy smart-contracts to Pravda
        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
                            "-i", TA_path, "--program-wallet", "wallets/TradableAsset-wallet.json"])
        print("TradableAsset.pravda deployed on fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b")

        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
                            "-i", auction_path, "--program-wallet", "wallets/auction-wallet.json"])
        print("Auction.pravda deployed on e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342")

    # Set up particular contract test
    def runContract(self, name, wallet, jsonifyOutput=True, silent=True):
        # Generate program wallet for the test
        call(["pravda", "gen", "address", "-o", "wallets/{}-test-wallet.json".format(name)])

        # Deploy the tester program
        address = json.loads(check_output(
            ["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
             "-i", pcall_file_path.format(name), "--program-wallet",
             "wallets/{}-test-wallet.json".format(name)]))['effects'][0]['address']

        if not silent:
            print("{}.pravda deployed on {}".format(name, address))

        # Run ASM code calling test program and save output to res
        res = check_output(
            'echo "push \\"test_{}\\" push x{} push 1 pcall"'.format(name, address) +
            '| pravda compile asm |'+
            'pravda broadcast run -w wallets/{}.json -l 9000000 '.format(wallet) +
            '--program-wallet wallets/{}-test-wallet.json'.format(name), shell=True)
        if jsonifyOutput:
            self.res = json.loads(res.decode('utf-8-sig'))["executionResult"]["success"]
        else:
            self.res = res

        # Clean up the program wallet
        call(["rm", "-rf", "wallets/{}-test-wallet.json".format(name)])

    # Test the auction cycle
    def test_auction_cycle(self):
        # Set up TradableAsset
        self.runContract("SetUpTradableAsset", "TradableAsset-wallet")
        print("TradableAsset was set up")

        # Set up auction
        self.runContract("SetUpAuction", "auction-wallet")
        print("Auction was set up")

        # Create a new lot
        self.runContract("NewLot", "test-wallet")
        self.assertEqual(self.res['stack'][0],
        'utf8.[' +
              '{"id": "1",' +
              '"creator": "8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f",' +
              '"gameId": "1",' +
              '"assetId": "1",' +
              '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",' +
              '"price": "200",' +
              '"closed": "0",' +
              '"buyer": "0000000000000000000000000000000000000000000000000000000000000000"},' +
              '{"id": "2",' +
              '"creator": "8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f",' +
              '"gameId": "1",' +
              '"assetId": "2",' +
              '"externalId": "0000000000000000000000000000000000000000000000000000000000000002",' +
              '"price": "200",' +
              '"closed": "0",' +
              '"buyer": "0000000000000000000000000000000000000000000000000000000000000000"}' +
        ']')

        print("2 lots were created")

        # Buy  a lot
        self.runContract("Buy", "test-wallet2", jsonifyOutput=False)
        # self.assertEqual(self.res['stack'][0],
        # 'utf8.{"id": "1",' +
        #       '"creator": "8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f",' +
        #       '"gameId": "1",' +
        #       '"assetId": "1",' +
        #       '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",' +
        #       '"price": "200",' +
        #       '"closed": "1",' +
        #       '"buyer": "edbfca5b9a253738634352c465b2f0ea1a2f280dbf5510bd83010798dd203996"}')
        print(self.res)
        print("A lot was bought")

        # Close a lot
        self.runContract("CloseLot", "test-wallet")
        
        print("Lot was closed")

    @classmethod
    def tearDownClass(self):
        # Terminate Pravda after testing
        print('Terminating pravda')
        time.sleep(2)
        self.pravda.send_signal(signal.SIGINT)
        self.pravda.wait()

        # Some clean-up
        print('Cleaning up the directory')
        call(["rm", "-rf", "pravda-data"])
        call(["rm", "-rf", TA_path])
        call(["rm", "-rf", auction_path])
        for test_call in self.test_calls:
            call(["rm", "-rf", pcall_file_path.format(test_call)])

if __name__ == '__main__':
    unittest.main()

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
        compile.compile_contracts(['setuppass', 'setupauction', 'newlot', 'bid', 'closelot', 'timeout'])

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

        # Deploy smart-contracts to Pravda
        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
                            "-i", "PASS.pravda", "--program-wallet", "wallets/pass-wallet.json"])
        print("PASS.pravda deployed on fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b")

        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
                            "-i", "Auction.pravda", "--program-wallet", "wallets/auction-wallet.json"])
        print("Auction.pravda deployed on e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342")

    # Set up particular contract test
    def runContract(self, name, wallet, jsonifyOutput=True, silent=True):
        # Generate program wallet for the test
        call(["pravda", "gen", "address", "-o", "wallets/{}-test-wallet.json".format(name)])

        # Deploy the tester program
        address = json.loads(check_output(
            ["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
             "-i", "pcalls/{}.pravda".format(name), "--program-wallet",
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
        # Set up PASS
        self.runContract("setuppass", "pass-wallet")
        print("PASS was set up")

        # Set up auction
        self.runContract("setupauction", "auction-wallet")
        print("Auction was set up")

        # Create a new lot
        self.runContract("newlot", "test-wallet")
        self.assertEqual(self.res['stack'][0],
        'utf8.{"creator": "8FC47DE7507F0881FB0133CBBD82733B69426B1B55904907F3DE3DBFB262210F",' +
              '"gameId": "1",' +
              '"assetId": "1",' +
              '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",' +
              '"startingPrice": "100",' +
              '"lastBid": "0",' +
              '"lastBidder": "0000000000000000000000000000000000000000000000000000000000000000",' +
              '"endTime": "1897084800",' +
              '"closed": "0"}')

        print("2 lots were created")

        # Bid on a lot
        self.runContract("bid", "test-wallet2")
        self.assertEqual(self.res['stack'][0],
        'utf8.{"creator": "8FC47DE7507F0881FB0133CBBD82733B69426B1B55904907F3DE3DBFB262210F",' +
              '"gameId": "1",' +
              '"assetId": "1",' +
              '"externalId": "0000000000000000000000000000000000000000000000000000000000000001",' +
              '"startingPrice": "100",' +
              '"lastBid": "200",' +
              '"lastBidder": "EDBFCA5B9A253738634352C465B2F0EA1A2F280DBF5510BD83010798DD203996",' +
              '"endTime": "1897084800",' +
              '"closed": "0"}')

        print("2 bids were made")

        # Close a lot
        self.runContract("closelot", "test-wallet")
        
        print("Lot was closed")
        
        # Timeout a lot
        self.runContract("timeout", "auction-wallet")
        
        print("Lost was timed out")

    @classmethod
    def tearDownClass(self):
        # Terminate Pravda after testing
        print('Terminating pravda')
        time.sleep(2)
        self.pravda.send_signal(signal.SIGINT)
        self.pravda.wait()

if __name__ == '__main__':
    unittest.main()

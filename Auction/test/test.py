#!/usr/bin/python

import unittest
from subprocess import call, Popen, DEVNULL, check_output
import signal
import json
import time
from datetime import datetime
TA_path = "../../TradableAsset/source/XC/bin/TradableXCAsset.pravda"
TA_GT_path = "../../TradableAsset/source/GT/bin/TradableGTAsset.pravda"
GT_path = "../source/bin/GameToken.pravda"
auction_path = "../source/bin/Auction.pravda"
pcall_file_path = "pcalls/{0}/bin/{0}.pravda"

class TestTradableAsset(unittest.TestCase):

    maxDiff = None
    # Pravda instance
    pravda = None
    # Result of setUp work
    res = None
    
    test_calls = ['SetUpTradableAsset', 'SetUpTradableGTAsset', 
                  'SetUpAuction', 'SetUpGT', 'NewLot', 'Buy', 'CloseLot']

    # Set up Pravda once before running the TestCase
    @classmethod
    def setUpClass(self):
        # Compile main & test contracts
        output = check_output(["dotnet", "publish", "../source/Auction.sln"], timeout=40)
        print("Programs compiled")

        
        call(["rm", "-rf", "pravda-data"], timeout=40)
        # Init new local pravda blockchain
        call(["pravda", "node", "init", "--local", "--coin-distribution", "test-coin-dist.json"], timeout=40)

        # Run Pravda node in a new subprocess
        print("Starting pravda node")
        self.pravda = Popen(["pravda", "node", "run"], stdout=DEVNULL)
        # Wait for it to load
        time.sleep(10)
        
        print("Deploying programs")

        # Deploy main smart-contracts to Pravda
        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
                            "-i", TA_path, "--program-wallet", "wallets/TradableAsset-wallet.json"], timeout=40)
        print("TradableXCAsset.pravda deployed on fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b")

        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
                            "-i", TA_GT_path, "--program-wallet", "wallets/TradableGTAsset-wallet.json"], timeout=40)
        print("TradableGTAsset.pravda deployed on 17e22f66979eca19a8b060a8bb759bfb3dbbce785a039e9e1ed01a54cc92161c")

        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
                            "-i", auction_path, "--program-wallet", "wallets/auction-wallet.json"], timeout=40)
        print("Auction.pravda deployed on e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342")

        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
                            "-i", GT_path, "--program-wallet", "wallets/GT-wallet.json"], timeout=40)
        print("GameToken.pravda deployed on 64a818e62d78f7b2642b0535db69c9b7e7aff0f12562110bdeeea082dc217f29")

    # Set up particular contract test
    def runContract(self, name, wallet, jsonifyOutput=True, silent=True):
        # Generate program wallet for the test
        call(["pravda", "gen", "address", "-o", "wallets/{}-test-wallet.json".format(name)], timeout=40)

        # Deploy the tester program
        res = check_output(
            ["pravda", "broadcast", "deploy", "-w", "wallets/payer-wallet.json", "-l", "9000000",
             "-i", pcall_file_path.format(name), "--program-wallet",
             "wallets/{}-test-wallet.json".format(name)], timeout=40)

        try:
            address = json.loads(res)['effects'][0]['address']
        except Exception:
            print(res)
            raise Exception

        if not silent:
            print("{}.pravda deployed on {}".format(name, address))

        # Run ASM code calling test program and save output to res
        res = check_output(
            'echo "push \\"test_{}\\" push x{} push 1 pcall"'.format(name, address) +
            '| pravda compile asm |'+
            'pravda broadcast run -w wallets/{}.json -l 9000000 '.format(wallet) +
            '--program-wallet wallets/{}-test-wallet.json'.format(name), shell=True, timeout=40)
        if jsonifyOutput:
            try:
                self.res = json.loads(res.decode('utf-8-sig'))["executionResult"]["success"]
            except Exception:
                print(res)
                raise Exception
        else:
            self.res = res

        # Clean up the program wallet
        call(["rm", "-rf", "wallets/{}-test-wallet.json".format(name)])

    # Test the auction cycle
    def test_auction_cycle(self):
        # Set up TradableAsset
        self.runContract("SetUpTradableAsset", "TradableAsset-wallet")
        print("TradableXCAsset was set up")

        self.runContract("SetUpTradableGTAsset", "TradableGTAsset-wallet")
        print("TradableGTAsset was set up")


        # Set up auction
        self.runContract("SetUpAuction", "auction-wallet")
        print("Auction was set up")
        
        # Set up GameToken
        self.runContract("SetUpGT", "GT-wallet")
        print("GameToken was set up")

        # Create 3 new lots
        self.runContract("NewLot", "test-wallet")
        num = int(self.res['stack'][0].split(".")[1])
        lots = list(map(int, self.res['heap'][num][1:]))

        # Assert first lot
        expected_result = \
            "{'utf8.<Price>k__BackingField': 'int64.200', " + \
            "'utf8.<Buyer>k__BackingField': 'bytes.0000000000000000000000000000000000000000000000000000000000000000', " + \
            "'utf8.<Id>k__BackingField': 'int64.1', " + \
            "'utf8.<IsGT>k__BackingField': 'int32.0', " + \
            "'utf8.<GameId>k__BackingField': 'int64.1', " + \
            "'utf8.<Closed>k__BackingField': 'int32.0', " + \
            "'utf8.<AssetClassId>k__BackingField': 'bytes.0000000000000000000000000000000000000000000000000000000000000001', " + \
            "'utf8.<AssetId>k__BackingField': 'int64.1', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f'}"
        self.assertEqual(expected_result, str(self.res['heap'][lots[0]]))

        # Assert second lot
        expected_result = \
            "{'utf8.<Price>k__BackingField': 'int64.200', " + \
            "'utf8.<Buyer>k__BackingField': 'bytes.0000000000000000000000000000000000000000000000000000000000000000', " + \
            "'utf8.<Id>k__BackingField': 'int64.2', " + \
            "'utf8.<IsGT>k__BackingField': 'int32.0', " + \
            "'utf8.<GameId>k__BackingField': 'int64.1', " + \
            "'utf8.<Closed>k__BackingField': 'int32.0', " + \
            "'utf8.<AssetClassId>k__BackingField': 'bytes.0000000000000000000000000000000000000000000000000000000000000002', " + \
            "'utf8.<AssetId>k__BackingField': 'int64.2', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f'}"
        self.assertEqual(expected_result, str(self.res['heap'][lots[1]]))

        # Assert third lot
        expected_result = \
            "{'utf8.<Price>k__BackingField': 'int64.200', " + \
            "'utf8.<Buyer>k__BackingField': 'bytes.0000000000000000000000000000000000000000000000000000000000000000', " + \
            "'utf8.<Id>k__BackingField': 'int64.3', " + \
            "'utf8.<IsGT>k__BackingField': 'int32.1', " + \
            "'utf8.<GameId>k__BackingField': 'int64.1', " + \
            "'utf8.<Closed>k__BackingField': 'int32.0', " + \
            "'utf8.<AssetClassId>k__BackingField': 'bytes.0000000000000000000000000000000000000000000000000000000000000001', " + \
            "'utf8.<AssetId>k__BackingField': 'int64.1', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f'}"
        self.assertEqual(expected_result, str(self.res['heap'][lots[2]]))

        print("3 lots were created")

        # Buy  a lot
        self.runContract("Buy", "test-wallet2")
        num = int(self.res['stack'][0].split(".")[1])
        expected_result = \
            "{'utf8.<Buyer>k__BackingField': 'bytes.edbfca5b9a253738634352c465b2f0ea1a2f280dbf5510bd83010798dd203996', " + \
            "'utf8.<Price>k__BackingField': 'int64.200', " + \
            "'utf8.<IsGT>k__BackingField': 'int32.0', " + \
            "'utf8.<Id>k__BackingField': 'int64.1', " + \
            "'utf8.<GameId>k__BackingField': 'int64.1', " + \
            "'utf8.<Closed>k__BackingField': 'int32.1', " + \
            "'utf8.<AssetClassId>k__BackingField': 'bytes.0000000000000000000000000000000000000000000000000000000000000001', " + \
            "'utf8.<AssetId>k__BackingField': 'int64.1', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f'}"
        self.assertEqual(expected_result, str(self.res['heap'][num]))

        # Close a lot
        self.runContract("CloseLot", "test-wallet")
        print("Lot was closed")

    @classmethod
    def tearDownClass(self):
        # Terminate Pravda after testing
        print('Terminating pravda')
        time.sleep(2)
        self.pravda.send_signal(signal.SIGKILL)
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
    sys.stdout.flush()

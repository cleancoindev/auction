#!/usr/bin/python
import sys
import unittest
from subprocess import call, Popen, DEVNULL, check_output
import signal
import json
import time

TA_path = "../source/XG/bin/TradableXGAsset.pravda"
pcall_file_path = "pcalls/{0}/bin/{0}.pravda"

compile_sripts = True

class TestTradableAsset(unittest.TestCase):

    maxDiff = None

    # Pravda instance
    pravda = None
    # Result of setUp work
    res = None

    test_calls = ['Emit', 'Transfer', 'Itemlist', 'UsersItems']

    # Set up Pravda once before running the TestCase
    @classmethod
    def setUpClass(self):
        # Compile main & test contracts
        if compile_sripts:
            output = check_output(["dotnet", "publish", "../source/TradableAsset.sln"], timeout=90)
            print("Programs compiled")

        # Delete current pravda blockchain data
        call(["rm", "-rf", "pravda-data"], timeout=40)
        # Init new local pravda blockchain
        call(["pravda", "node", "init", "--local", "--coin-distribution", "test-coin-dist.json"], timeout=40)
        print("New pravda node initialized")

        # Run Pravda node in a new subprocess
        print("Starting pravda node")
        self.pravda = Popen(["pravda", "node", "run"], stderr=DEVNULL, stdout=DEVNULL)
        # Wait for it to load
        time.sleep(40)

        print("Deploying programs")

        # Deploy smart-contract to Pravda
        res = check_output(["pravda", "broadcast", "deploy", "-w", "wallets/test-wallet.json", "-l", "9000000",
                            "-i", TA_path, "--program-wallet", "wallets/program-wallet.json"], timeout=40)
        print("TradableXGAsset.pravda deployed on fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b")

    # Set up particular contract test
    def setUp(self):
        # Get the name of the test
        name = self._testMethodName[5:]
        # Generate program wallet for the test
        call(["pravda", "gen", "address", "-o", "wallets/{}-test-wallet.json".format(name)], timeout=40)

        # Deploy the tester program
        address = None
        res = check_output(
            ["pravda", "broadcast", "deploy", "-w", "wallets/test-wallet.json", "-l", "9000000",
             "-i", pcall_file_path.format(name), "--program-wallet",
             "wallets/{}-test-wallet.json".format(name)], timeout=40)
        try:
            address = json.loads(res)['effects'][0]['address']
        except Exception:
            print(res)
            raise Exception

        print("{}.pravda deployed on {}".format(name, address))

        # Run ASM code calling test program and save output to res
        res = check_output(
            'echo "push \\"test_{}\\" push x{} push 1 pcall"'.format(name, address) +
            '| pravda compile asm |'+
            'pravda broadcast run -w wallets/program-wallet.json -l 9000000 '+
            '--program-wallet wallets/{}-test-wallet.json'.format(name), shell=True, timeout=40)
        try:
            self.res = json.loads(res.decode('utf-8-sig'))["executionResult"]["success"]
        except Exception:
            print(res)
            raise Exception

        # Clean up the program wallet
        call(["rm", "-rf", "wallets/{}-test-wallet.json".format(name)], timeout=40)

    # Test if assets can be emitted
    def test_Emit(self):
        expected_result = \
            "{'utf8.<ItemInstanceId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000002', " + \
            "'utf8.<Id>k__BackingField': 'int64.1', " + \
            "'utf8.<ItemClassId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000001', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342'}"
        self.assertEqual(str(self.res["heap"][0]), expected_result)
        print("Emit tested")

    # Test if assets can be transfered
    def test_Transfer(self):
        expected_result = \
            "{'utf8.<ItemInstanceId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000002', " + \
            "'utf8.<Id>k__BackingField': 'int64.4', " + \
            "'utf8.<ItemClassId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000001', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342'}"
        self.assertEqual(str(self.res["heap"][0]), expected_result)
        print("Transfer tested")

    # Test if item list is working
    def test_Itemlist(self):
        self.assertEqual(len(self.res["stack"]), 0)
        print("Itemlist tested")

    # Test if getting all user items works
    def test_UsersItems(self):
        assets = list(map(int, self.res["heap"][int(self.res["stack"][0].split(".")[1])][1:]))

        expected_result = \
            "{'utf8.<ItemInstanceId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000001', " + \
            "'utf8.<Id>k__BackingField': 'int64.5', " + \
            "'utf8.<ItemClassId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000001', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.a1fe824f193bcee32f33b9e01245bd41f05a157eca73daf65d70ebd27430836d'}"

        self.assertEqual(str(self.res["heap"][assets[0]]), expected_result)

        expected_result = \
            "{'utf8.<ItemInstanceId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000002', " + \
            "'utf8.<Id>k__BackingField': 'int64.6', " + \
            "'utf8.<ItemClassId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000002', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.a1fe824f193bcee32f33b9e01245bd41f05a157eca73daf65d70ebd27430836d'}"

        self.assertEqual(str(self.res["heap"][assets[1]]), expected_result)

        expected_result = \
            "{'utf8.<ItemInstanceId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000003', " + \
            "'utf8.<Id>k__BackingField': 'int64.7', " + \
            "'utf8.<ItemClassId>k__BackingField': " + \
                "'bytes.0000000000000000000000000000000000000000000000000000000000000003', " + \
            "'utf8.<Owner>k__BackingField': 'bytes.a1fe824f193bcee32f33b9e01245bd41f05a157eca73daf65d70ebd27430836d'}"

        self.assertEqual(str(self.res["heap"][assets[2]]), expected_result)

        print("UsersItems tested")

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
        for test_call in self.test_calls:
            call(["rm", "-rf", pcall_file_path.format(test_call)])

if __name__ == '__main__':
    unittest.main()
    sys.stdout.flush()

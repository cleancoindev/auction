using Expload.Pravda;
using System;

namespace PcallNamespace {

    [Program]
    public class Auction {
        /*
        Official Expload Auction Program
        */

        public static void Main() { }

        /*
        PASS adresses of different games storage
        */

        // Last id given to a game
        public UInt32 lastGameId = 0;

        // Mapping storing adresses of games 
        public Mapping<UInt32, Bytes> gamesAddresses =
            new Mapping<UInt32, Bytes>();

        // Add a new game address
        public UInt32 addGame(Bytes address) {
            // Only Auction Owner can do this
            assertIsAuctionOwner();
            // Add game address to the storage
            gamesAddresses.put(lastGameId++, address);
            return lastGameId;
        }

        // Get game address by its game id
        private Bytes getGameAddress(UInt32 id) {
            return gamesAddresses.getDefault(id, Bytes.VOID_ADDRESS);
        }

        /*
        Lot objects storage
        */

        // Last id given to a lot
        public UInt32 lastLotId = 0;

        // Mapping storing lot objects
        public Mapping<UInt32, Lot> Lots =
            new Mapping<UInt32, Lot>();

        /*
        Lot ids belonging to particular users storage
        */

        // Mapping storing lot ids of a particular user
        public Mapping<string, UInt32> userLots =
            new Mapping<string, UInt32>();

        // Mapping storing the amount of user lots
        public Mapping<Bytes, UInt32> userLotsCount =
            new Mapping<Bytes, uint>();

        public UInt32 getUserLotId(Bytes address, UInt32 number) {
            // We can't get more lots than user has
            if(number >= userLotsCount.getDefault(address, 0)){
                Error.Throw("This asset doesn't exist!");
            }
            string key = getUserLotKey(address, number);
            return userLots.get(key);
        }

        // Get the key for userLots mapping
        private string getUserLotKey(Bytes address, UInt32 number) {
            return (BytesToHex(address) + System.Convert.ToString(number));
        }

        /*
        Lot ids selling particular assets storage
        */

        // Mapping storing lot ids selling a particular asset
        public Mapping<string, UInt32> assetLots =
            new Mapping<string, UInt32>();

        // Mapping storing the amount of particular asset lots
        public Mapping<string, UInt32> assetLotsCount =
            new Mapping<string, UInt32>();

        // IMPORTANT: Asset id = External Asset id (see PASS.cs)
        public UInt32 getAssetLotId(UInt32 gameId, Bytes assetId, UInt32 number) {
            // We can't get more lots than asset has
            if(number >= assetLotsCount.getDefault(getAssetCountKey(gameId, assetId), 0)){
                Error.Throw("This asset doesn't exist!");
            }
            string key = getAssetLotKey(gameId, assetId, number);
            return assetLots.get(key);
        }

        // Get the key for assetLotsCount mapping
        private string getAssetCountKey(UInt32 gameId, Bytes assetId) {
            return (System.Convert.ToString(gameId) + BytesToHex(assetId));
        }

        // Get the key for assetLots mapping
        private string getAssetLotKey(UInt32 gameId, Bytes assetId, UInt32 number) {
            return (System.Convert.ToString(gameId) + BytesToHex(assetId) + System.Convert.ToString(number));
        }

        /*
        Permission-checkers
        */

        // Checks if caller is the owner of the contract
        // (if it's a call from game's server)
        private void assertIsAuctionOwner(){
            if (Info.Sender() != Info.ProgramAddress()){
                Error.Throw("Only owner of the program can do that.");
            }
        }

        /*
        Some string & bytes operations
        */

        private string HexPart(int b) {
            if (b == 0)
                return "0";
            else if (b == 1)
                return "1";
            else if (b == 2)
                return "2";
            else if (b == 3)
                return "3";
            else if (b == 4)
                return "4";
            else if (b == 5)
                return "5";
            else if (b == 6)
                return "6";
            else if (b == 7)
                return "7";
            else if (b == 8)
                return "8";
            else if (b == 9)
                return "9";
            else if (b == 10)
                return "A";
            else if (b == 11)
                return "B";
            else if (b == 12)
                return "C";
            else if (b == 13)
                return "D";
            else if (b == 14)
                return "E";
            else if (b == 15)
                return "F";
            return "";
        }

        private string ByteToHex(byte b)
        {
            return HexPart(b / 16) + HexPart(b % 16);
        }

        private string BytesToHex(Bytes bytes)
        {
            string res = "";
            for (int i = 0; i < bytes.Length(); i++) {
                res += ByteToHex(bytes[i]);
            }
            return res;
        }
    }

    public class Lot {
        /*
        Class defining auction lot
        */

        // Address of lot creator
        Bytes creator;

        // Blockchain id of the asset sold (see PASS.cs)
        UInt32 id;

        // External game id of the asset sold (see PASS.cs)
        UInt32 externalId;

        // Starting price of the asset
        UInt32 startingPrice;

        // Last (highest) lot bid
        UInt32 lastBid;

        // The owner of the last bid
        Bytes lastBidder;

        // UNIX timestamp for lot start
        UInt64 StartTime;

        // Lot duration, seconds
        UInt32 Duration;

        // If the lot is already closed
        bool closed = false;
    }
}
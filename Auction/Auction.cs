namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class Auction {
        /*
        Official Expload Auction Program
        */

        public static void Main(){ }

        /*
        PASS adresses of different games storage
        */

        // Last id given to a game
        public UInt32 lastGameId = 0;

        // Mapping storing adresses of games 
        public Mapping<UInt32, Bytes> gamesAddresses =
            new Mapping<UInt32, Bytes>();

        // Add a new game address
        public UInt32 addGame(Bytes address){
            // Only Auction Owner can do this
            assertIsAuctionOwner();
            // Add game address to the storage
            gamesAddresses.put(lastGameId++, address);
            return lastGameId;
        }

        // Get game address by its game id
        private Bytes getGameAddress(UInt32 id){
            return gamesAddresses.getDefault(id, Bytes.VOID_ADDRESS);
        }

        /*
        Parsing lot objects
        */

        // Parse arguments into Lot object
        private Lot ParseLot(
            Bytes creator, UInt32 gameId, UInt32 assetId,
            Bytes externalId, UInt32 startingPrice, UInt32 endTime
        ){
            var lot = new Lot();
            lot.creator = creator;
            lot.gameId = gameId;
            lot.assetId = assetId;
            lot.externalId = externalId;
            lot.startingPrice = startingPrice;
            lot.endTime = endTime;
            return lot;
        }

        // Dump Lot into JSON
        private string DumpLot(Lot lot){
            return
            "{" +
                "\"creator\": \""       + BytesToHex(lot.creator)                    + "\"," +
                "\"gameId\": \""        + System.Convert.ToString(lot.gameId)        + "\"," + 
                "\"assetId\": \""       + System.Convert.ToString(lot.assetId)       + "\"," +
                "\"externalId\": \""    + BytesToHex(lot.externalId)                 + "\"," +
                "\"startingPrice\": \"" + System.Convert.ToString(lot.startingPrice) + "\"," +
                "\"endTime\": \""       + System.Convert.ToString(lot.endTime)       + "\""  +
            "}";
        }

        /*
        Lot objects storage
        */

        // Last id given to a lot
        public UInt32 lastLotId = 0;

        // Mapping storing lot objects
        public Mapping<UInt32, Lot> Lots =
            new Mapping<UInt32, Lot>();

        // Get lot by its id
        private Lot getLot(UInt32 id){
            return Lots.getDefault(id, new Lot());
        }

        // Get jsonified lot data
        public string getLotData(UInt32 id){
            return DumpLot(getLot(id));
        }

        /*
        Lot ids belonging to particular users storage
        */

        // Mapping storing lot ids of a particular user
        public Mapping<string, UInt32> userLots =
            new Mapping<string, UInt32>();

        // Mapping storing the amount of user lots
        public Mapping<Bytes, UInt32> userLotsCount =
            new Mapping<Bytes, uint>();

        public UInt32 getUserLotId(Bytes address, UInt32 number){
            // We can't get more lots than user has
            if(number >= userLotsCount.getDefault(address, 0)){
                Error.Throw("This asset doesn't exist!");
            }
            string key = getUserLotKey(address, number);
            return userLots.get(key);
        }

        // Get the key for userLots mapping
        private string getUserLotKey(Bytes address, UInt32 number){
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
        public UInt32 getAssetLotId(UInt32 gameId, Bytes externalId, UInt32 number){
            // We can't get more lots than asset has
            if(number >= assetLotsCount.getDefault(getAssetCountKey(gameId, externalId), 0)){
                Error.Throw("This asset doesn't exist!");
            }
            string key = getAssetLotKey(gameId, externalId, number);
            return assetLots.get(key);
        }

        // Get the key for assetLotsCount mapping
        private string getAssetCountKey(UInt32 gameId, Bytes externalId){
            return (System.Convert.ToString(gameId) + BytesToHex(externalId));
        }

        // Get the key for assetLots mapping
        private string getAssetLotKey(UInt32 gameId, Bytes externalId, UInt32 number){
            return (System.Convert.ToString(gameId) + BytesToHex(externalId) + System.Convert.ToString(number));
        }

        /*
        Permission-checkers
        */

        // Checks if caller is the owner of the contract
        // (if it's a call from game's server)
        private void assertIsAuctionOwner(){
            if (Info.Sender() != Info.ProgramAddress()){
                Error.Throw("Only program owner can do this.");
            }
        }

        // Checks if caller owns a particular asset
        private void assertIsItemOwner(UInt32 gameId, UInt32 assetId){
            Bytes gameAddress = getGameAddress(gameId);
            Bytes assetOwner = ProgramHelper.Program<PASS>(gameAddress).getXCAssetOwner(assetId);
            if(Info.Sender() != assetOwner){
                Error.Throw("Only asset owner can do this.");
            }
        }

        // Checks if caller is a creator of particular lot
        private void assertIsLotCreator(UInt32 lotId){
            if(Info.Sender() != getLot(lotId).creator){
                Error.Throw("Only lot creator can do this.");
            }
        }

        /*
        Interacting with the storage
        */

        /// <summary>
        /// Creates a new lot with desired parameters,
        /// asset ownership is transfered to auction wallet.
        /// </summary>
        /// <param name="gameId"> Id of the game the asset is from </param>
        /// <param name="assetId"> Blockchain id of the asset sold (see PASS.cs) </param>
        /// <param name="startingPrice"> Starting price of the lot, can't equal 0 </param>
        /// <param name="endTime"> UNIX timestamp for the end of the sales </param>
        /// <returns>
        /// Created lot id
        /// </returns>
        /// <remarks>
        /// If the lot is closed by its creator, the bids are given back and the
        /// asset is returned to the owner. If the lot is not closed before the timeout,
        /// highest bidder gets the asset and lot creator gets the highest bid.
        /// If endTime timestamp is in the past (regarding lot timeout daemon),
        /// the lot is to be closed almost instantly and the lot 
        /// is not to be shown in Expload acution UI.
        /// </remarks>
        public UInt32 createLot(
            UInt32 gameId, UInt32 assetId, 
            UInt32 startingPrice, UInt32 endTime
        ){
            // Check if user has the item he wants to sell
            assertIsItemOwner(gameId, assetId);

            // Check if the starting price is legit
            if(startingPrice == 0){
                Error.Throw("Starting price can't equal 0.");
            }

            // Get game address
            Bytes gameAddress = getGameAddress(gameId);

            // Get item external id
            Bytes externalId = ProgramHelper.Program<PASS>(gameAddress).getXCAssetExternalId(assetId);

            // Transfer the asset to auction's wallet (so user can't use it)
            ProgramHelper.Program<PASS>(gameAddress).TransferXCAsset(assetId, Info.ProgramAddress());

            // Create lot object and put it into main storage
            Lot lot = ParseLot(Info.Sender(), gameId, assetId, externalId, startingPrice, endTime);
            UInt32 lotId = lastLotId;
            Lots.put(lastLotId++, lot);

            // Put the lot into user storage
            UInt32 userStorageLastId = userLotsCount.getDefault(Info.Sender(), 0);
            string userLotsKey = getUserLotKey(Info.Sender(), userStorageLastId);
            userLots.put(userLotsKey, lotId);
            userLotsCount.put(Info.Sender(), userStorageLastId + 1);

            // Put the lot into particular asset storage
            string assetLotsCountKey = getAssetCountKey(gameId, externalId);
            UInt32 assetCount = assetLotsCount.getDefault(assetLotsCountKey, 0);
            string assetLotsKey = getAssetLotKey(gameId, externalId, assetCount);
            assetLots.put(assetLotsKey, lotId);
            assetLotsCount.put(assetLotsKey, assetCount+1);

            // TODO: add event

            return lotId;
        }

        // Minimum amount of lot bid increment
        public UInt32 minBidIncrement = 5;

        // Sets new minimum bid increment
        // Can only be done by program owner
        public void setMinBidIncrement(UInt32 increment){
            assertIsAuctionOwner();
            minBidIncrement = increment;
        }

        /// <summary>
        /// Places a new bid on desired lot
        /// </summary>
        /// <param name="lotId"> Id of the lot </param>
        /// <param name="bid"> Desired bid </param>
        /// <remarks>
        /// If there haven't been any bids on the lot before, the bid
        /// should be equal to the starting price or be greater than it.
        /// When one's bid is outbid, the coins are returned.
        /// </remarks>
        public void makeBid(UInt32 lotId, UInt32 bid){
            // Get the lot object
            Lot lot = getLot(lotId);

            // Check if the lot is not closed yet
            if(lot.closed){
                Error.Throw("The lot is already closed.");
            }

            // Check if the bid is legit
            if(!(
                bid >= lot.startingPrice && lot.lastBid == 0 
                || lot.lastBid != 0 && bid >= lot.lastBid + minBidIncrement
            )) {
                Error.Throw("The bid is not legit.");
            }
            
            // Transfer the outbid bet back to the bidder
            if(lot.lastBid != 0){
                Actions.TransferFromProgram(lot.lastBidder, lot.lastBid);
            }

            // Transfer new bid to the auction
            Actions.Transfer(Info.ProgramAddress(), bid);

            // Alter the lot state and write it to the storage
            lot.lastBid = bid;
            lot.lastBidder = Info.Sender();
            Lots.put(lotId, lot);
        }

        /// <summary>
        /// Cancel the bidding, return the bids to bidders
        /// and the asset to its owner.
        /// </summary>
        /// <param name="lotId"> Id of the lot </param>
        /// <remarks>
        /// If the lot is closed, it is not to be shown
        /// in Expload Autction UI (except for lot creator's lot history).
        /// The lot is permanently closed and archieved, it can't be reopened.
        /// </remarks>
        public void closeLot(UInt32 lotId){
            // Check if sender has permission to do this
            assertIsLotCreator(lotId);

            // Get the lot object
            Lot lot = getLot(lotId);

            // Change the lot state and write it to the storage
            lot.closed = true;
            Lots.put(lotId, lot);

            // Return the last bid to the bidder
            Actions.TransferFromProgram(lot.lastBidder, lot.lastBid);

            // Return the asset to the owner
            Bytes gameAddress = getGameAddress(lot.gameId);
            ProgramHelper.Program<PASS>(gameAddress).TransferXCAsset(lot.assetId, lot.creator);
        }

        /// <summary>
        /// Special method for auction's timeout daemon program
        /// which constantly looks for expired lots.
        /// When an expired lot is found, the highest bidder
        /// gets the asset and the lot creator gets the highest bid.
        /// </summary>
        /// <param name="lotId"> Id of the lot </param>
        /// <remarks>
        /// Though the timeout worker may be a little late for 
        /// contract expiration (max - couple of seconds),
        /// outdated lots are not shown in Expload UI, and
        /// latency is too small and random for it to be exploited.
        /// </remarks>
        public void timeoutLot(UInt32 lotId){
            // Only the daemon script (program owner) can do this
            assertIsAuctionOwner();

            // Get the lot object
            Lot lot = getLot(lotId);

            // Change the lot state and write it to the storage
            lot.closed = true;
            Lots.put(lotId, lot);

            // Give the coins to lot creator
            Actions.TransferFromProgram(lot.creator, lot.lastBid);

            // Transfer asset to the highest bidder
            Bytes gameAddress = getGameAddress(lot.gameId);
            ProgramHelper.Program<PASS>(gameAddress).TransferXCAsset(lot.assetId, lot.lastBidder);
        }

        /*
        Some string & bytes operations
        */

        private string HexPart(int b){
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
            for (int i = 0; i < bytes.Length(); i++){
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
        public Bytes creator;

        // Id of the game the asset is from
        public UInt32 gameId;

        // Blockchain id of the asset sold (see PASS.cs)
        public UInt32 assetId;

        // External game id of the asset sold (see PASS.cs)
        public Bytes externalId;

        // Starting price of the asset
        public UInt32 startingPrice;

        // Last (highest) lot bid
        public UInt32 lastBid = 0;

        // The owner of the last bid
        public Bytes lastBidder = Bytes.VOID_ADDRESS;

        // UNIX timestamp for lot end
        public UInt32 endTime;

        // If the lot is already closed
        public bool closed = false;
    }
}
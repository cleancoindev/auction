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
            gamesAddresses[++lastGameId] = address;
            return lastGameId;
        }

        // Get game address by its game id
        private Bytes getGameAddress(UInt32 id){
            return gamesAddresses.GetOrDefault(id, Bytes.VOID_ADDRESS);
        }

        /*
        Parsing lot objects
        */

        // Parse arguments into Lot object
        private Lot ParseLot(
            UInt32 id, Bytes creator, UInt32 gameId, 
            UInt32 assetId, Bytes externalId, UInt32 price
        ){
            var lot = new Lot();
            lot.id = id;
            lot.creator = creator;
            lot.gameId = gameId;
            lot.assetId = assetId;
            lot.externalId = externalId;
            lot.price = price;
            return lot;
        }

        // Dump Lot into JSON
        private string DumpLot(Lot lot){
            return
            "{" +
                "\"id\": \""            + System.Convert.ToString(lot.id)      + "\"," + 
                "\"creator\": \""       + StdLib.BytesToHex(lot.creator)       + "\"," +
                "\"gameId\": \""        + System.Convert.ToString(lot.gameId)  + "\"," + 
                "\"assetId\": \""       + System.Convert.ToString(lot.assetId) + "\"," +
                "\"externalId\": \""    + StdLib.BytesToHex(lot.externalId)    + "\"," +
                "\"price\": \""         + System.Convert.ToString(lot.price)   + "\"," +
                "\"closed\": \""        + System.Convert.ToString(lot.closed)  + "\"," +
                "\"buyer\": \""         + StdLib.BytesToHex(lot.buyer)         + "\"" +
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
            return Lots.GetOrDefault(id, new Lot());
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

        private UInt32 _getUserLotId(Bytes address, UInt32 number){
            // We can't get more lots than user has
            if(number >= userLotsCount.GetOrDefault(address, 0)){
                Error.Throw("This user's lot doesn't exist!");
            }
            string key = getUserLotKey(address, number);
            return userLots.GetOrDefault(key, 0);
        }

        public UInt32 getUserLotId(Bytes address, UInt32 number){
            return _getUserLotId(address, number);
        }

        // Get the key for userLots mapping
        private string getUserLotKey(Bytes address, UInt32 number){
            return (StdLib.BytesToHex(address) + System.Convert.ToString(number));
        }

        // Get all of users' lots jsonified
        public string getUserLotsData(Bytes address){
            string result = "[";
            UInt32 amount = userLotsCount.GetOrDefault(address, 0);
            for(UInt32 num = 0; num < amount; num++){
                result += DumpLot(getLot(_getUserLotId(address, num)));
                if(num < amount - 1){
                    result += ",";
                }
            }
            return result + "]";
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
        private UInt32 _getAssetLotId(UInt32 gameId, Bytes externalId, UInt32 number){
            // We can't get more lots than asset has
            if(number >= assetLotsCount.GetOrDefault(getAssetCountKey(gameId, externalId), 0)){
                Error.Throw("This asset's lot doesn't exist!");
            }
            string key = getAssetLotKey(gameId, externalId, number);
            return assetLots.GetOrDefault(key, 0);
        }

        public UInt32 getAssetLotId(UInt32 gameId, Bytes externalId, UInt32 number){
            return _getAssetLotId(gameId, externalId, number);
        }

        // Get the key for assetLotsCount mapping
        private string getAssetCountKey(UInt32 gameId, Bytes externalId){
            return (System.Convert.ToString(gameId) + StdLib.BytesToHex(externalId));
        }

        // Get the key for assetLots mapping
        private string getAssetLotKey(UInt32 gameId, Bytes externalId, UInt32 number){
            return (System.Convert.ToString(gameId) + StdLib.BytesToHex(externalId) + System.Convert.ToString(number));
        }

        // Get all of asset lots jsonified
        public string getAssetLotsData(UInt32 gameId, Bytes externalId){
            string result = "[";
            UInt32 amount = assetLotsCount.GetOrDefault(getAssetCountKey(gameId, externalId), 0);
            for(UInt32 num = 0; num < amount; num++){
                result += DumpLot(getLot(_getAssetLotId(gameId, externalId, num)));
                if(num < amount - 1){
                    result += ",";
                }
            }
            return result + "]";
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
        /// <param name="price"> Price of the lot, can't equal 0 </param>
        /// <returns>
        /// Created lot id
        /// </returns>
        public UInt32 createLot(
            UInt32 gameId, UInt32 assetId, UInt32 price
        ){
            // Check if user has the item he wants to sell
            assertIsItemOwner(gameId, assetId);

            // Check if the starting price is legit
            if(price == 0){
                Error.Throw("Price can't equal 0.");
            }

            // Get game address
            Bytes gameAddress = getGameAddress(gameId);

            // Get item external id
            Bytes externalId = ProgramHelper.Program<PASS>(gameAddress).getXCAssetExternalId(assetId);

            // Transfer the asset to auction's wallet (so user can't use it)
            ProgramHelper.Program<PASS>(gameAddress).TransferXCAsset(assetId, Info.ProgramAddress());

            // Create lot object and put it into main storage
            UInt32 lotId = ++lastLotId;
            Lot lot = ParseLot(lotId, Info.Sender(), gameId, assetId, externalId, price);
            Lots[lastLotId] = lot;

            // Put the lot into user storage
            UInt32 userStorageLastId = userLotsCount.GetOrDefault(Info.Sender(), 0);
            string userLotsKey = getUserLotKey(Info.Sender(), userStorageLastId);
            userLots[userLotsKey] = lotId;
            userLotsCount[Info.Sender()] = userStorageLastId + 1;

            // Put the lot into particular asset storage
            string assetLotsCountKey = getAssetCountKey(gameId, externalId);
            UInt32 assetCount = assetLotsCount.GetOrDefault(assetLotsCountKey, 0);
            string assetLotsKey = getAssetLotKey(gameId, externalId, assetCount);
            assetLots[assetLotsKey] = lotId;
            assetLotsCount[assetLotsCountKey] = assetCount+1;

            // Emit an event
            Log.Event("lotCreated", DumpLot(lot));

            return lotId;
        }

        /// <summary>
        /// Buy desired lot
        /// </summary>
        /// <param name="lotId"> Id of the lot </param>
        public void buyLot(UInt32 lotId){
            // Get the lot object
            Lot lot = getLot(lotId);

            // Check if the lot is not closed yet
            if(lot.closed){
                Error.Throw("The lot is already closed.");
            }

            // Take the money from buyer
            Actions.Transfer(Info.ProgramAddress(), lot.price);

            // Transfer the asset to buyer
            Bytes gameAddress = getGameAddress(lot.gameId);
            ProgramHelper.Program<PASS>(gameAddress).TransferXCAsset(lot.assetId, Info.Sender());

            // Alter the lot state and write it to the storage
            lot.closed = true;
            lot.buyer = Info.Sender();
            Lots[lotId] = lot;

            // Emit an event
            Log.Event("lotBought", DumpLot(lot));
        }

        /// <summary>
        /// Cancel the lot, return asset to owner
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

            // Check if the lot is already closed
            if(lot.closed){
                Error.Throw("The lot is already closed.");
            }

            // Change the lot state and write it to the storage
            lot.closed = true;
            Lots[lotId] = lot;

            // Return the asset to the owner
            Bytes gameAddress = getGameAddress(lot.gameId);
            ProgramHelper.Program<PASS>(gameAddress).TransferXCAsset(lot.assetId, lot.creator);

            // Emit an event
            Log.Event("lotClosed", DumpLot(lot));
        }
    }

    public class Lot {
        /*
        Class defining auction lot
        */

        // Id of the lot
        public UInt32 id { get; set; } = 0;

        // Address of lot creator
        public Bytes creator { get; set; } = Bytes.VOID_ADDRESS;

        // Id of the game the asset is from
        public UInt32 gameId { get; set; } = 0;

        // Blockchain id of the asset sold (see PASS.cs)
        public UInt32 assetId { get; set; } = 0;

        // External game id of the asset sold (see PASS.cs)
        public Bytes externalId { get; set; } = Bytes.VOID_ADDRESS;

        // Starting price of the asset
        public UInt32 price { get; set; } = 0;

        // If the lot is already closed
        public bool closed { get; set; } = false;

        // Buyer's address
        public Bytes buyer { get; set; } = Bytes.VOID_ADDRESS;
    }
}
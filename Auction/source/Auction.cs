namespace Expload {

    using Pravda;
    using Standarts;
    using System;

    [Program]
    public class Auction {
        /*
        Official Expload Auction Program
        */

        public static void Main(){ }

        /*
        TradableAsset addresses of different games storage
        */

        // Last id given to a game
        private long _lastGameId = 0;

        // Mapping storing addresses of games 
        private Mapping<long, Bytes> _gamesAddresses =
            new Mapping<long, Bytes>();

        /// <summary>
        /// Add a new game to auction
        /// </summary>
        /// <param name="address"> Address of game's TradableAsset program </param>
        /// <returns>
        /// New game id
        /// </returns>
        public long AddGame(Bytes address){
            // Only Auction Owner can do this
            AssertIsAuctionOwner();
            // Add game address to the storage
            _gamesAddresses[++_lastGameId] = address;
            return _lastGameId;
        }

        // Get game address by its game id
        private Bytes GetGameAddress(long id){
            return _gamesAddresses.GetOrDefault(id, Bytes.VOID_ADDRESS);
        }

        /*
        Parsing lot objects
        */

        // Dump Lot into JSON
        private string DumpLot(Lot lot){
            return
            "{" +
                "\"id\": \""            + System.Convert.ToString(lot.Id)      + "\"," + 
                "\"creator\": \""       + StdLib.BytesToHex(lot.Owner)       + "\"," +
                "\"gameId\": \""        + System.Convert.ToString(lot.GameId)  + "\"," + 
                "\"assetId\": \""       + System.Convert.ToString(lot.AssetId) + "\"," +
                "\"externalId\": \""    + StdLib.BytesToHex(lot.ExternalId)    + "\"," +
                "\"price\": \""         + System.Convert.ToString(lot.Price)   + "\"," +
                "\"closed\": \""        + System.Convert.ToString(lot.Closed)  + "\"," +
                "\"buyer\": \""         + StdLib.BytesToHex(lot.Buyer)         + "\"" +
            "}";
        }

        /*
        Lot objects storage
        */

        // Last id given to a lot
        private long _lastLotId = 0;

        // Mapping storing lot objects
        private Mapping<long, Lot> _lots =
            new Mapping<long, Lot>();

        // Get lot by its id
        private Lot GetLot(long id){
            return _lots.GetOrDefault(id, new Lot());
        }

        /// <summary>
        /// Get JSONified lot data
        /// </summary>
        /// <param name="id"> Lot id </param>
        /// <returns>
        /// JSON object
        /// </returns>
        public string GetLotData(long id){
            return DumpLot(GetLot(id));
        }

        /*
        Lot ids belonging to particular users storage
        */

        // Mapping storing lot ids of a particular user
        private Mapping<string, long> _userLots =
            new Mapping<string, long>();

        // Mapping storing the amount of user lots
        private Mapping<Bytes, long> _userLotsCount =
            new Mapping<Bytes, long>();

        private long _getUserLotId(Bytes address, long number){
            // We can't get more lots than user has
            if(number >= _userLotsCount.GetOrDefault(address, 0)){
                Error.Throw("This user's lot doesn't exist!");
            }
            var key = GetUserLotKey(address, number);
            return _userLots.GetOrDefault(key, 0);
        }

        /// <summary>
        /// Get lot id of
        /// lot belonging to a particular user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <param name="number"> Serial number in storage </param>
        /// <returns>
        /// Lot id
        /// </returns>
        public long GetUserLotId(Bytes address, long number){
            return _getUserLotId(address, number);
        }

        // Get the key for userLots mapping
        private string GetUserLotKey(Bytes address, long number){
            return (StdLib.BytesToHex(address) + System.Convert.ToString(number));
        }

        /// <summary>
        /// Get JSONified lists of lots
        /// belonging to a particular user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// JSON object
        /// </returns>
        public string GetUserLotsData(Bytes address){
            var result = "[";
            var amount = _userLotsCount.GetOrDefault(address, 0);
            for(long num = 0; num < amount; num++){
                result += DumpLot(GetLot(_getUserLotId(address, num)));
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
        private Mapping<string, long> _assetLots =
            new Mapping<string, long>();

        // Mapping storing the amount of particular asset lots
        private Mapping<string, long> _assetLotsCount =
            new Mapping<string, long>();

        // IMPORTANT: Asset id = External Asset id (see TradableAsset.cs)
        private long _getAssetLotId(long gameId, Bytes externalId, long number){
            // We can't get more lots than asset has
            if(number >= _assetLotsCount.GetOrDefault(GetAssetCountKey(gameId, externalId), 0)){
                Error.Throw("This asset's lot doesn't exist!");
            }
            var key = GetAssetLotKey(gameId, externalId, number);
            return _assetLots.GetOrDefault(key, 0);
        }

        /// <summary>
        /// Get lot id of
        /// a particular asset lot
        /// </summary>
        /// <param name="gameId"> Id of the game the asset is from </param>
        /// <param name="externalId"> External id of the asset sold (see TradableAsset.cs) </param>
        /// <param name="number"> Serial number in storage </param>
        /// <returns>
        /// Lot id
        /// </returns>
        public long GetAssetLotId(long gameId, Bytes externalId, long number){
            return _getAssetLotId(gameId, externalId, number);
        }

        // Get the key for assetLotsCount mapping
        private string GetAssetCountKey(long gameId, Bytes externalId){
            return (System.Convert.ToString(gameId) + StdLib.BytesToHex(externalId));
        }

        // Get the key for assetLots mapping
        private string GetAssetLotKey(long gameId, Bytes externalId, long number){
            return (System.Convert.ToString(gameId) + StdLib.BytesToHex(externalId) + System.Convert.ToString(number));
        }

        /// <summary>
        /// Get JSONified lists of lots of
        /// a particular asset
        /// </summary>
        /// <param name="gameId"> Id of the game the asset is from </param>
        /// <param name="externalId"> External id of the asset sold (see TradableAsset.cs) </param>
        /// <returns>
        /// JSON object
        /// </returns>
        public string GetAssetLotsData(long gameId, Bytes externalId){
            var result = "[";
            var amount = _assetLotsCount.GetOrDefault(GetAssetCountKey(gameId, externalId), 0);
            for(long num = 0; num < amount; num++){
                result += DumpLot(GetLot(_getAssetLotId(gameId, externalId, num)));
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
        private void AssertIsAuctionOwner(){
            if (Info.Sender() != Info.ProgramAddress()){
                Error.Throw("Only program owner can do this.");
            }
        }

        // Checks if caller owns a particular asset
        private void AssertIsItemOwner(long gameId, long assetId){
            var gameAddress = GetGameAddress(gameId);
            var assetOwner = ProgramHelper.Program<TradableAsset>(gameAddress).GetXCAssetOwner(assetId);
            if(Info.Sender() != assetOwner){
                Error.Throw("Only asset owner can do this.");
            }
        }

        // Checks if caller is a creator of particular lot
        private void AssertIsLotCreator(long lotId){
            if(Info.Sender() != GetLot(lotId).Owner){
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
        /// <param name="assetId"> Blockchain id of the asset sold (see TradableAsset.cs) </param>
        /// <param name="price"> Price of the lot, can't equal 0 </param>
        /// <returns>
        /// Created lot id
        /// </returns>
        public long CreateLot(
            long gameId, long assetId, long price
        ){
            // Check if user has the item he wants to sell
            AssertIsItemOwner(gameId, assetId);

            // Check if the starting price is legit
            if(price == 0){
                Error.Throw("Price can't equal 0.");
            }

            // Get game address
            var gameAddress = GetGameAddress(gameId);

            // Get item external id
            var externalId = ProgramHelper.Program<TradableAsset>(gameAddress).GetXCAssetExternalId(assetId);

            // Transfer the asset to auction's wallet (so user can't use it)
            ProgramHelper.Program<TradableAsset>(gameAddress).TransferXCAsset(assetId, Info.ProgramAddress());

            // Create lot object and put it into main storage
            var lotId = ++_lastLotId;
            var lot = new Lot(lotId, Info.Sender(), gameId, assetId, externalId, price);
            _lots[_lastLotId] = lot;

            // Put the lot into user storage
            var userStorageLastId = _userLotsCount.GetOrDefault(Info.Sender(), 0);
            var userLotsKey = GetUserLotKey(Info.Sender(), userStorageLastId);
            _userLots[userLotsKey] = lotId;
            _userLotsCount[Info.Sender()] = userStorageLastId + 1;

            // Put the lot into particular asset storage
            var assetLotsCountKey = GetAssetCountKey(gameId, externalId);
            var assetCount = _assetLotsCount.GetOrDefault(assetLotsCountKey, 0);
            var assetLotsKey = GetAssetLotKey(gameId, externalId, assetCount);
            _assetLots[assetLotsKey] = lotId;
            _assetLotsCount[assetLotsCountKey] = assetCount+1;

            // Emit an event
            Log.Event("lotCreated", DumpLot(lot));

            return lotId;
        }

        /// <summary>
        /// Buy desired lot
        /// </summary>
        /// <param name="lotId"> Id of the lot </param>
        public void BuyLot(long lotId){
            // Get the lot object
            Lot lot = GetLot(lotId);

            // Check if the lot is not closed yet
            if(lot.Closed){
                Error.Throw("The lot is already closed.");
            }

            // Take the money from buyer
            Actions.Transfer(Info.ProgramAddress(), lot.Price);

            // Transfer the asset to buyer
            Bytes gameAddress = GetGameAddress(lot.GameId);
            ProgramHelper.Program<TradableAsset>(gameAddress).TransferXCAsset(lot.AssetId, Info.Sender());

            // Alter the lot state and write it to the storage
            lot.Closed = true;
            lot.Buyer = Info.Sender();
            _lots[lotId] = lot;

            // Emit an event
            Log.Event("lotBought", DumpLot(lot));
        }

        /// <summary>
        /// Cancel the lot, return asset to owner
        /// </summary>
        /// <param name="lotId"> Id of the lot </param>
        /// <remarks>
        /// If the lot is closed, it is not to be shown
        /// in Expload Auction UI (except for lot creator's lot history).
        /// The lot is permanently closed and archived, it can't be reopened.
        /// </remarks>
        public void CloseLot(long lotId){
            // Check if sender has permission to do this
            AssertIsLotCreator(lotId);

            // Get the lot object
            var lot = GetLot(lotId);

            // Check if the lot is already closed
            if(lot.Closed){
                Error.Throw("The lot is already closed.");
            }

            // Change the lot state and write it to the storage
            lot.Closed = true;
            _lots[lotId] = lot;

            // Return the asset to the owner
            var gameAddress = GetGameAddress(lot.GameId);
            ProgramHelper.Program<TradableAsset>(gameAddress).TransferXCAsset(lot.AssetId, lot.Owner);

            // Emit an event
            Log.Event("lotClosed", DumpLot(lot));
        }
    }

    public class Lot {
        /*
        Class defining auction lot
        */
        
        public Lot(
            long id, Bytes owner, long gameId, 
            long assetId, Bytes externalId, long price
        ){
            this.Id = id;
            this.Owner = owner;
            this.GameId = gameId;
            this.AssetId = assetId;
            this.ExternalId = externalId;
            this.Price = price;
        }
        
        public Lot() { }
        
        // If the lot is already closed
        public bool Closed { get; set; } = false;

        // Id of the lot
        public long Id { get; set; } = 0;

        // Id of the game the asset is from
        public long GameId { get; set; } = 0;

        // Blockchain id of the asset sold (see TradableAsset.cs)
        public long AssetId { get; set; } = 0;

        // Starting price of the asset
        public long Price { get; set; } = 0;
        
        // Address of lot creator
        public Bytes Owner { get; set; } = Bytes.VOID_ADDRESS;

        // Buyer's address
        public Bytes Buyer { get; set; } = Bytes.VOID_ADDRESS;
        
        // External game id of the asset sold (see TradableAsset.cs)
        public Bytes ExternalId { get; set; } = Bytes.VOID_ADDRESS;
    }
}
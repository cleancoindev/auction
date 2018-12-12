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
        private uint _lastGameId = 0;

        // Mapping storing addresses of games 
        private Mapping<uint, Bytes> _gamesAddresses =
            new Mapping<uint, Bytes>();

        // Add a new game address
        public uint AddGame(Bytes address){
            // Only Auction Owner can do this
            AssertIsAuctionOwner();
            // Add game address to the storage
            _gamesAddresses[++_lastGameId] = address;
            return _lastGameId;
        }

        // Get game address by its game id
        private Bytes GetGameAddress(uint id){
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
        private uint _lastLotId = 0;

        // Mapping storing lot objects
        private Mapping<uint, Lot> _lots =
            new Mapping<uint, Lot>();

        // Get lot by its id
        private Lot GetLot(uint id){
            return _lots.GetOrDefault(id, new Lot());
        }

        // Get JSONified lot data
        public string GetLotData(uint id){
            return DumpLot(GetLot(id));
        }

        /*
        Lot ids belonging to particular users storage
        */

        // Mapping storing lot ids of a particular user
        private Mapping<string, uint> _userLots =
            new Mapping<string, uint>();

        // Mapping storing the amount of user lots
        private Mapping<Bytes, uint> _userLotsCount =
            new Mapping<Bytes, uint>();

        private uint _getUserLotId(Bytes address, uint number){
            // We can't get more lots than user has
            if(number >= _userLotsCount.GetOrDefault(address, 0)){
                Error.Throw("This user's lot doesn't exist!");
            }
            var key = GetUserLotKey(address, number);
            return _userLots.GetOrDefault(key, 0);
        }

        public uint GetUserLotId(Bytes address, uint number){
            return _getUserLotId(address, number);
        }

        // Get the key for userLots mapping
        private string GetUserLotKey(Bytes address, uint number){
            return (StdLib.BytesToHex(address) + System.Convert.ToString(number));
        }

        // Get all of users' lots JSONified
        public string GetUserLotsData(Bytes address){
            var result = "[";
            var amount = _userLotsCount.GetOrDefault(address, 0);
            for(uint num = 0; num < amount; num++){
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
        private Mapping<string, uint> _assetLots =
            new Mapping<string, uint>();

        // Mapping storing the amount of particular asset lots
        private Mapping<string, uint> _assetLotsCount =
            new Mapping<string, uint>();

        // IMPORTANT: Asset id = External Asset id (see TradableAsset.cs)
        private uint _getAssetLotId(uint gameId, Bytes externalId, uint number){
            // We can't get more lots than asset has
            if(number >= _assetLotsCount.GetOrDefault(GetAssetCountKey(gameId, externalId), 0)){
                Error.Throw("This asset's lot doesn't exist!");
            }
            var key = GetAssetLotKey(gameId, externalId, number);
            return _assetLots.GetOrDefault(key, 0);
        }

        public uint GetAssetLotId(uint gameId, Bytes externalId, uint number){
            return _getAssetLotId(gameId, externalId, number);
        }

        // Get the key for assetLotsCount mapping
        private string GetAssetCountKey(uint gameId, Bytes externalId){
            return (System.Convert.ToString(gameId) + StdLib.BytesToHex(externalId));
        }

        // Get the key for assetLots mapping
        private string GetAssetLotKey(uint gameId, Bytes externalId, uint number){
            return (System.Convert.ToString(gameId) + StdLib.BytesToHex(externalId) + System.Convert.ToString(number));
        }

        // Get all of asset lots JSONified
        public string GetAssetLotsData(uint gameId, Bytes externalId){
            var result = "[";
            var amount = _assetLotsCount.GetOrDefault(GetAssetCountKey(gameId, externalId), 0);
            for(uint num = 0; num < amount; num++){
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
        private void AssertIsItemOwner(uint gameId, uint assetId){
            var gameAddress = GetGameAddress(gameId);
            var assetOwner = ProgramHelper.Program<TradableAsset>(gameAddress).GetXCAssetOwner(assetId);
            if(Info.Sender() != assetOwner){
                Error.Throw("Only asset owner can do this.");
            }
        }

        // Checks if caller is a creator of particular lot
        private void AssertIsLotCreator(uint lotId){
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
        public uint CreateLot(
            uint gameId, uint assetId, uint price
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
        public void BuyLot(uint lotId){
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
        public void CloseLot(uint lotId){
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
            uint id, Bytes owner, uint gameId, 
            uint assetId, Bytes externalId, uint price
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
        public uint Id { get; set; } = 0;

        // Id of the game the asset is from
        public uint GameId { get; set; } = 0;

        // Blockchain id of the asset sold (see TradableAsset.cs)
        public uint AssetId { get; set; } = 0;

        // Starting price of the asset
        public uint Price { get; set; } = 0;
        
        // Address of lot creator
        public Bytes Owner { get; set; } = Bytes.VOID_ADDRESS;

        // Buyer's address
        public Bytes Buyer { get; set; } = Bytes.VOID_ADDRESS;
        
        // External game id of the asset sold (see TradableAsset.cs)
        public Bytes ExternalId { get; set; } = Bytes.VOID_ADDRESS;
    }
}
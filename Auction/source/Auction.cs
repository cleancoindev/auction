namespace Expload {

    using Pravda;
    using Standards;
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

        // Last id given to a game's GT TradableAsset program
        private long _lastGTGameId = 0;
        
        // Last id given to a game's XC TradableAsset program
        private long _lastXCGameId = 0;

        // Mapping storing addresses of games' GT TradableAsset programs
        private Mapping<long, Bytes> _gamesGTAddresses =
            new Mapping<long, Bytes>();
        
        // Mapping storing addresses of games' XC TradableAsset programs
        private Mapping<long, Bytes> _gamesXCAddresses =
            new Mapping<long, Bytes>();
        
        /// <summary>
        /// Add a new game's TradableAsset to auction
        /// </summary>
        /// <param name="address"> Address of game's TradableAsset program </param>
        /// <param name="isGT"> True if the program handles GT assets, false if XC </param>
        /// <returns>
        /// New game id
        /// </returns>
        public long AddGame(Bytes address, bool isGT){
            // Only Auction Owner can do this
            AssertIsAuctionOwner();
            // Add game address to the storage
            if (isGT)
            {
                _gamesGTAddresses[++_lastGTGameId] = address;
                return _lastGTGameId;
            }
            else
            {
                _gamesXCAddresses[++_lastXCGameId] = address;
                return _lastXCGameId;
            }
        }

        // Get game address by its game id
        private Bytes GetGameAddress(long id, bool isGT){
            if (isGT)
            {
                return _gamesGTAddresses.GetOrDefault(id, Bytes.VOID_ADDRESS);
            }
            else
            {
                return _gamesXCAddresses.GetOrDefault(id, Bytes.VOID_ADDRESS);
            }
        }
        
        // GameToken program address
        private Bytes GTAddress = Bytes.VOID_ADDRESS;

        /// <summary>
        /// Set GameToken program address
        /// </summary>
        /// <param name="address"> GameToken program address </param>
        public void SetGTAddress(Bytes address)
        {
            // Only Auction Owner can set auction address
            AssertIsAuctionOwner();
            // Actually set the address
            GTAddress = address;
        }

        /// <summary>
        /// Get GameToken program address
        /// </summary>
        /// <returns> GameToken program address </returns>
        public Bytes GetGTAddress()
        {
            return GTAddress;
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
        public Lot GetLotData(long id){
            return GetLot(id);
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
        public Lot[] GetUserLotsData(Bytes address){
            int amount = (int)_userLotsCount.GetOrDefault(address, 0);
            var result = new Lot[amount];
            for(int num = 0; num < amount; num++){
                result[num] = GetLot(_getUserLotId(address, num));
            }
            return result;
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
        public Lot[] GetAssetLotsData(long gameId, Bytes externalId){
            int amount = (int)_assetLotsCount.GetOrDefault(GetAssetCountKey(gameId, externalId), 0);
            var result = new Lot[amount];
            for(int num = 0; num < amount; num++){
                result[num] = GetLot(_getAssetLotId(gameId, externalId, num));
            }
            return result;
        }

        /*
        Permission-checkers
        */

        // Checks if caller is the owner of the program
        // (if it's a call from game's server)
        private void AssertIsAuctionOwner(){
            if (Info.Sender() != Info.ProgramAddress()){
                Error.Throw("Only program owner can do this.");
            }
        }

        // Checks if caller owns a particular asset
        private void AssertIsItemOwner(long gameId, long assetId, bool isGT){
            var gameAddress = GetGameAddress(gameId, isGT);
            
            var assetOwner = isGT ? 
                ProgramHelper.Program<TradableGTAsset>(gameAddress).GetGTAssetOwner(assetId) : 
                ProgramHelper.Program<TradableXCAsset>(gameAddress).GetXCAssetOwner(assetId); 
            
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
        /// <param name="isGT"> True if asset is GT, false if XC </param>
        /// <param name="assetId"> Blockchain id of the asset sold (see TradableAsset.cs) </param>
        /// <param name="price"> Price of the lot, can't equal 0 </param>
        /// <returns>
        /// Created lot id
        /// </returns>
        public long CreateLot(
            long gameId, bool isGT, long assetId, long price
        ){
            // Check if user has the item he wants to sell
            AssertIsItemOwner(gameId, assetId, isGT);

            // Check if the starting price is legit
            if(price == 0){
                Error.Throw("Price can't equal 0.");
            }

            // Get game address
            var gameAddress = GetGameAddress(gameId, isGT);

            // Get item external id
            var externalId = isGT ? 
                ProgramHelper.Program<TradableGTAsset>(gameAddress).GetGTAssetExternalId(assetId) : 
                ProgramHelper.Program<TradableXCAsset>(gameAddress).GetXCAssetExternalId(assetId); 

            // Transfer the asset to auction's wallet (so user can't use it)
            if (isGT)
            {
                ProgramHelper.Program<TradableGTAsset>(gameAddress).TransferGTAsset(assetId, Info.ProgramAddress());
            }
            else
            {
                ProgramHelper.Program<TradableXCAsset>(gameAddress).TransferXCAsset(assetId, Info.ProgramAddress());
            }

            // Create lot object and put it into main storage
            var lotId = ++_lastLotId;
            var lot = new Lot(lotId, Info.Sender(), gameId, isGT, assetId, externalId, price);
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
            Log.Event("lotCreated", lot);

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

            // Take the money from buyer and ransfer the asset to him
            var gameAddress = GetGameAddress(lot.GameId, lot.IsGT);
            if (lot.IsGT)
            {
                Int32 price = (Int32)lot.Price;
                ProgramHelper.Program<GameToken>(GTAddress).Spend(Info.ProgramAddress(), price);
                ProgramHelper.Program<TradableGTAsset>(gameAddress).TransferGTAsset(lot.AssetId, Info.Sender());
            }
            else
            {
                Actions.Transfer(Info.ProgramAddress(), lot.Price);
                ProgramHelper.Program<TradableXCAsset>(gameAddress).TransferXCAsset(lot.AssetId, Info.Sender());
            }

            // Alter the lot state and write it to the storage
            lot.Closed = true;
            lot.Buyer = Info.Sender();
            _lots[lotId] = lot;

            // Emit an event
            Log.Event("lotBought", lot);
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
            var gameAddress = GetGameAddress(lot.GameId, lot.IsGT);
            if (lot.IsGT)
            {
                ProgramHelper.Program<TradableGTAsset>(gameAddress).TransferGTAsset(lot.AssetId, lot.Owner);
            }
            else
            {
                ProgramHelper.Program<TradableXCAsset>(gameAddress).TransferXCAsset(lot.AssetId, lot.Owner);
            }

            // Emit an event
            Log.Event("lotClosed", lot);
        }
    }

    public class Lot {
        /*
        Class defining auction lot
        */
        
        public Lot(
            long id, Bytes owner, long gameId, bool isGT,
            long assetId, Bytes externalId, long price
        ){
            Id = id;
            Owner = owner;
            GameId = gameId;
            IsGT = isGT;
            AssetId = assetId;
            ExternalId = externalId;
            Price = price;
        }
        
        public Lot() { }
        
        // Id of the lot
        public long Id { get; set; } = 0;
               
        // Address of lot creator
        public Bytes Owner { get; set; } = Bytes.VOID_ADDRESS;
        
        // Id of the game the asset is from
        public long GameId { get; set; } = 0;
        
        // Type of the asset: true if GT, false if XC
        public bool IsGT { get; set; } = false;
        
        // Blockchain id of the asset sold (see TradableAsset.cs)
        public long AssetId { get; set; } = 0;
                
        // External game id of the asset sold (see TradableAsset.cs)
        public Bytes ExternalId { get; set; } = Bytes.VOID_ADDRESS;
        
        // Starting price of the asset
        public long Price { get; set; } = 0;
        
        // If the lot is already closed
        public bool Closed { get; set; } = false;
        
        // Buyer's address
        public Bytes Buyer { get; set; } = Bytes.VOID_ADDRESS;
    }
}
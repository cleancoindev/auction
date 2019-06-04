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

        // Last id given to a game's XG TradableAsset program
        private long _lastXGGameId = 0;
        
        // Last id given to a game's XP TradableAsset program
        private long _lastXPGameId = 0;

        // Mapping storing addresses of games' XG TradableAsset programs
        private Mapping<long, Bytes> _gamesXGAddresses =
            new Mapping<long, Bytes>();
        
        // Mapping storing addresses of games' XP TradableAsset programs
        private Mapping<long, Bytes> _gamesXPAddresses =
            new Mapping<long, Bytes>();
        
        /// <summary>
        /// Add a new game's TradableAsset to auction
        /// </summary>
        /// <param name="address"> Address of game's TradableAsset program </param>
        /// <param name="isXG"> True if the program handles XG assets, false if XP </param>
        /// <returns>
        /// New game id
        /// </returns>
        public long AddGame(Bytes address, bool isXG){
            // Only Auction Owner can do this
            AssertIsAuctionOwner();
            // Add game address to the storage
            if (isXG)
            {
                _gamesXGAddresses[++_lastXGGameId] = address;
                return _lastXGGameId;
            }
            else
            {
                _gamesXPAddresses[++_lastXPGameId] = address;
                return _lastXPGameId;
            }
        }

        // Get game address by its game id
        private Bytes _GetGameAddress(long id, bool isXG){
            if (isXG)
            {
                return _gamesXGAddresses.GetOrDefault(id, Bytes.VOID_ADDRESS);
            }
            else
            {
                return _gamesXPAddresses.GetOrDefault(id, Bytes.VOID_ADDRESS);
            }
        }

        public Bytes GetGameAddress(long id, bool isXG){
            return _GetGameAddress(id, isXG);
        }
        
        // XGold program address
        private Bytes XGAddress = Bytes.VOID_ADDRESS;

        /// <summary>
        /// Set XGold program address
        /// </summary>
        /// <param name="address"> XGold program address </param>
        public void SetXGAddress(Bytes address)
        {
            // Only Auction Owner can set auction address
            AssertIsAuctionOwner();
            // Actually set the address
            XGAddress = address;
        }

        /// <summary>
        /// Get XGold program address
        /// </summary>
        /// <returns> XGold program address </returns>
        public Bytes GetXGAddress()
        {
            return XGAddress;
        }

        // Percent of commission (default 5)
        private int CommissionPercent = 5;

        /// <summary>
        /// Set up commission for auction
        /// </summary>
        /// <param name="percent"> Percent of commission </param>
        public void SetCommission(int percent)
        {
            AssertIsAuctionOwner();

            if (percent < 0 && percent > 40)
            {
                Error.Throw("Commission percent can be in the range from 0 to 40");
            }

            CommissionPercent = percent;
        }

        /// <summary>
        /// Get percent of commission
        /// </summary>
        /// <returns> Percent of commission </returns>
        public int GetCommission()
        {
            return CommissionPercent;
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
        /// Get lot data
        /// </summary>
        /// <param name="id"> Lot id </param>
        /// <returns>
        /// Lot object
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
        /// Get list of lots
        /// belonging to a particular user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// List of lot objects
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

        private long _getAssetLotId(long gameId, Bytes classId, long number){
            // We can't get more lots than asset has
            if(number >= _assetLotsCount.GetOrDefault(GetAssetCountKey(gameId, classId), 0)){
                Error.Throw("This asset's lot doesn't exist!");
            }
            var key = GetAssetLotKey(gameId, classId, number);
            return _assetLots.GetOrDefault(key, 0);
        }

        /// <summary>
        /// Get lot id of
        /// a particular asset lot
        /// </summary>
        /// <param name="gameId"> Id of the game the asset is from </param>
        /// <param name="classId"> Class id of the asset sold (see TradableAsset.cs) </param>
        /// <param name="number"> Serial number in storage </param>
        /// <returns>
        /// Lot id
        /// </returns>
        public long GetAssetLotId(long gameId, Bytes classId, long number){
            return _getAssetLotId(gameId, classId, number);
        }

        // Get the key for assetLotsCount mapping
        private string GetAssetCountKey(long gameId, Bytes classId){
            return (System.Convert.ToString(gameId) + StdLib.BytesToHex(classId));
        }

        // Get the key for assetLots mapping
        private string GetAssetLotKey(long gameId, Bytes classId, long number){
            return (System.Convert.ToString(gameId) + StdLib.BytesToHex(classId) + System.Convert.ToString(number));
        }

        /// <summary>
        /// Get list of lots of
        /// a particular asset
        /// </summary>
        /// <param name="gameId"> Id of the game the asset is from </param>
        /// <param name="classId"> Class id of the asset sold (see TradableAsset.cs) </param>
        /// <returns>
        /// List of lot objects
        /// </returns>
        public Lot[] GetAssetLotsData(long gameId, Bytes classId){
            int amount = (int)_assetLotsCount.GetOrDefault(GetAssetCountKey(gameId, classId), 0);
            var result = new Lot[amount];
            for(int num = 0; num < amount; num++){
                result[num] = GetLot(_getAssetLotId(gameId, classId, num));
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
        private void AssertIsItemOwner(long gameId, long assetId, bool isXG){
            var gameAddress = _GetGameAddress(gameId, isXG);
            
            var assetOwner = isXG ? 
                ProgramHelper.Program<TradableXGAsset>(gameAddress).GetXGAssetOwner(assetId) : 
                ProgramHelper.Program<TradableXPAsset>(gameAddress).GetXPAssetOwner(assetId); 
            
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
        /// <param name="isXG"> True if asset is XG, false if XP </param>
        /// <param name="assetId"> Blockchain id of the asset sold (see TradableAsset.cs) </param>
        /// <param name="price"> Price of the lot, can't equal 0 </param>
        /// <returns>
        /// Created lot id
        /// </returns>
        public long CreateLot(
            long gameId, bool isXG, long assetId, long price
        ){
            // Check if user has the item he wants to sell
            AssertIsItemOwner(gameId, assetId, isXG);

            // Check if the starting price is legit
            if(price <= 0){
                Error.Throw("Incorrect price.");
            }

            // Get game address
            var gameAddress = _GetGameAddress(gameId, isXG);

            // Get item class id
            var classId = isXG ? 
                ProgramHelper.Program<TradableXGAsset>(gameAddress).GetXGAssetClassId(assetId) : 
                ProgramHelper.Program<TradableXPAsset>(gameAddress).GetXPAssetClassId(assetId); 

            // Transfer the asset to auction's wallet (so user can't use it)
            if (isXG)
            {
                ProgramHelper.Program<TradableXGAsset>(gameAddress).TransferXGAsset(assetId, Info.ProgramAddress());
            }
            else
            {
                ProgramHelper.Program<TradableXPAsset>(gameAddress).TransferXPAsset(assetId, Info.ProgramAddress());
            }

            // Get percent commssion
            var gameCommissionPercent = isXG ?
                ProgramHelper.Program<TradableXGAsset>(gameAddress).GetCommission() :
                ProgramHelper.Program<TradableXPAsset>(gameAddress).GetCommission();

            // Create lot object and put it into main storage
            var lotId = ++_lastLotId;

            var lot = new Lot(
                lotId, Info.Sender(), gameId, isXG, assetId, classId, 
                price, CommissionPercent, gameCommissionPercent, Info.LastBlockTime());

            _lots[_lastLotId] = lot;

            // Put the lot into user storage
            var userStorageLastId = _userLotsCount.GetOrDefault(Info.Sender(), 0);
            var userLotsKey = GetUserLotKey(Info.Sender(), userStorageLastId);
            _userLots[userLotsKey] = lotId;
            _userLotsCount[Info.Sender()] = userStorageLastId + 1;

            // Put the lot into particular asset storage
            var assetLotsCountKey = GetAssetCountKey(gameId, classId);
            var assetCount = _assetLotsCount.GetOrDefault(assetLotsCountKey, 0);
            var assetLotsKey = GetAssetLotKey(gameId, classId, assetCount);
            _assetLots[assetLotsKey] = lotId;
            _assetLotsCount[assetLotsCountKey] = assetCount+1;

            // Emit an event
            Log.Event("lotCreated", lot.Id);

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

            // Take the money from buyer and transfer the asset to him
            Bytes gameAddress = _GetGameAddress(lot.GameId, lot.IsXG);

            long ownerFee = (long)(lot.Price / (1 + ((double)(lot.AuctionCommission + lot.GameCommission)) / 100));
            long gameFee = (long)(ownerFee * (1 + ((double)lot.GameCommission) / 100)) - ownerFee;

            if (lot.IsXG)
            {
                ProgramHelper.Program<XGold>(XGAddress).Spend(Info.ProgramAddress(), lot.Price);
                ProgramHelper.Program<XGold>(XGAddress).Refund(Info.ProgramAddress(), lot.Owner, ownerFee);
                
                if (gameFee > 0)
                {
                    ProgramHelper.Program<XGold>(XGAddress).Refund(Info.ProgramAddress(), gameAddress, gameFee);
                }

                ProgramHelper.Program<TradableXGAsset>(gameAddress).TransferXGAsset(lot.AssetId, Info.Sender());
            }
            else
            {
                Actions.Transfer(Info.ProgramAddress(), lot.Price);
                Actions.TransferFromProgram(lot.Owner, ownerFee);

                if (gameFee > 0)
                {
                    Actions.TransferFromProgram(gameAddress, gameFee);
                }

                ProgramHelper.Program<TradableXPAsset>(gameAddress).TransferXPAsset(lot.AssetId, Info.Sender());
            }

            // Alter the lot state and write it to the storage
            lot.Closed = true;
            lot.Buyer = Info.Sender();
            lot.PurchaseTime = Info.LastBlockTime();
            _lots[lotId] = lot;

            // Put the lot into buyer storage (for history log)
            var userStorageLastId = _userLotsCount.GetOrDefault(Info.Sender(), 0);
            var userLotsKey = GetUserLotKey(Info.Sender(), userStorageLastId);
            _userLots[userLotsKey] = lotId;
            _userLotsCount[Info.Sender()] = userStorageLastId + 1;

            // Emit an event
            Log.Event("lotBought", lot.Id);
        }

        /// <summary>
        /// Cancel the lot, return asset to owner
        /// </summary>
        /// <param name="lotId"> Id of the lot </param>
        /// <remarks>
        /// If the lot is closed, it is not to be shown
        /// in Expload Auction UI (eXPept for lot creator's lot history).
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
            var gameAddress = _GetGameAddress(lot.GameId, lot.IsXG);
            if (lot.IsXG)
            {
                ProgramHelper.Program<TradableXGAsset>(gameAddress).TransferXGAsset(lot.AssetId, lot.Owner);
            }
            else
            {
                ProgramHelper.Program<TradableXPAsset>(gameAddress).TransferXPAsset(lot.AssetId, lot.Owner);
            }

            // Emit an event
            Log.Event("lotClosed", lot.Id);
        }
    }

    public class Lot {
        /*
        Class defining auction lot
        */
        
        public Lot(
            long id, Bytes owner, long gameId, bool isXG,
            long assetId, Bytes classId, long price,
            long auctionCommission, long gameCommission, long creationTime
        ){
            Id = id;
            Owner = owner;
            GameId = gameId;
            IsXG = isXG;
            AssetId = assetId;
            AssetClassId = classId;
            Price = price;
            AuctionCommission = auctionCommission;
            GameCommission = gameCommission;
            CreationTime = creationTime;
        }
        
        public Lot() { }
        
        // Id of the lot
        public long Id { get; set; } = 0;
               
        // Address of lot creator
        public Bytes Owner { get; set; } = Bytes.VOID_ADDRESS;
        
        // Id of the game the asset is from
        public long GameId { get; set; } = 0;
        
        // Type of the asset: true if XG, false if XP
        public bool IsXG { get; set; } = false;
        
        // Blockchain id of the asset sold (see TradableAsset.cs)
        public long AssetId { get; set; } = 0;
                
        // Asset class id of the asset sold (see TradableAsset.cs)
        public Bytes AssetClassId { get; set; } = Bytes.VOID_ADDRESS;
        
        // Starting price of the asset
        public long Price { get; set; } = 0;

        // Starting commission of the asset by game
        public long GameCommission { get; set; } = 0;

        // Starting commission of the asset by auction
        public long AuctionCommission { get; set; } = 0;

        // If the lot is already closed
        public bool Closed { get; set; } = false;
        
        // Buyer's address
        public Bytes Buyer { get; set; } = Bytes.VOID_ADDRESS;

        // UNIX timestamp of lot creation time

        public long CreationTime { get; set; } = 0;

        // UNIX timestamp of lot purchase time

        public long PurchaseTime { get; set; } = 0;
    }
}
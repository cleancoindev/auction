namespace Expload {

    using Standards;
    using Pravda;
    using System;

    [Program]
    public class TradableXPAsset : ITradableXPAsset
    {
        /*
        This program defines a common standard
        for storing game assets in Pravda blockchain
        and interacting with them.

        The pattern should be followed for Expload Platform
        to be able to interact with game contracts,
        for example, in order to use Expload assets auction.

        ---------------------------------

        Each asset has three types of IDs:

        ---------------------------------

        Blockchain id - used to navigate storage mappings in
        TradableAsset contract, has no in-game meaning
        Can't equal 0 (0 means no asset exists)

        ---------------------------------

        Item class id - the id which shows what particular
        in-game item class is behind the asset stored in the contract

        E.g. two identical in-game swords but with different upgrades
        or belonging to different players have same class id

        ---------------------------------

        Item instance id - the id which shows what particular
        in-game item instance is behind the asset stored in the contract

        E.g. two identical in-game swords but with different upgrades
        or belonging to different players have diferent instance id

        ---------------------------------
        
        GetItemClassMeta and GetItemInstanceMeta methods should be modified, so
        given ItemClassId or ItemInstanceId they should return a link
        to JSON with following format:

        {
            "name": <itemName>,
            "desc": <itemShortDescription>,
            "pic": <itemPictureURL>,
            "misc": <miscData*>
        }

        *miscData - full item description, item stats, etc.

        ---------------------------------

        This class represents a XP asset (can only be bought and sold for XGold)
        
        */

        public static void Main(){ }

        // Last id given to an XP asset (id=0 is invalid)
        private long _lastXPId = 0;

        /*
        Main asset storage
        */

        // Mapping storing XP assets
        // This mapping's key is asset's blockchain id
        private Mapping<long, Asset> _XPAssets =
            new Mapping<long, Asset>();

        private Asset GetXPAsset(long id){
            return _XPAssets[id];
        }

        /// <summary>
        /// Get XP asset data
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Asset object
        /// </returns>
        public Asset GetXPAssetData(long id){
            return GetXPAsset(id);
        }

        /// <summary>
        /// Get XP asset owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Owner address
        /// </returns>
        public Bytes GetXPAssetOwner(long id){
            return GetXPAsset(id).Owner;
        }

        /// <summary>
        /// Get XP asset class id
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Class id
        /// </returns>
        public Bytes GetXPAssetClassId(long id){
            return GetXPAsset(id).ItemClassId;
        }

        /*
        Users' asset storage
        */

        // Mapping storing XP assets ids belonging to a user
        // Key is the concatenation of user address and asset number in his storage
        private Mapping<string, long> _XPUsersAssetIds =
            new Mapping<string, long>();

        // Mapping storing XP user's asset counter
        private Mapping<Bytes, long> _XPUsersAssetCount =
            new Mapping<Bytes, long>();

        /// <summary>
        /// Get amount of XP assets belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// Asset amount
        /// </returns>
        public long GetUsersXPAssetCount(Bytes address){
            return _XPUsersAssetCount.GetOrDefault(address, 0);
        }

        // Get one of user's XP assets
        private long _getUsersXPAssetId(Bytes address, long number){
            // We can't get more assets than user owns
            if(number >= _XPUsersAssetCount.GetOrDefault(address, 0)){
                Error.Throw("This asset doesn't exist!");
            }
            var key = GetUserAssetKey(address, number);
            return _XPUsersAssetIds[key];
        }

        /// <summary>
        /// Get asset id of a particular XP asset belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <param name="number"> Asset serial number </param>
        /// <returns>
        /// Asset id
        /// </returns>
        public long GetUsersXPAssetId(Bytes address, long number){
            return _getUsersXPAssetId(address, number);
        }

        /// <summary>
        /// Get list of XP assets
        /// belonging to a particular user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// List of asset objects
        /// </returns>
        public Asset[] GetUsersAllXPAssetsData(Bytes address){
            int amount = (int)_XPUsersAssetCount.GetOrDefault(address, 0);
            var result = new Asset[amount];
            for(int num = 0; num < amount; num++){
                result[num] = GetXPAsset(_getUsersXPAssetId(address, num));
            }
            return result;
        }

        // Get key for users asset storage
        private string GetUserAssetKey(Bytes address, long number){
            return (StdLib.BytesToHex(address) + System.Convert.ToString(number));
        }

        /*
        Permission-checkers
        */

        // Checks if caller is the owner of the contract
        // (if it's a call from game's server)
        private void AssertIsGameOwner(){
            if (Info.Sender() != Info.ProgramAddress()){
                Error.Throw("Only owner of the program can do that.");
            }
        }

        // Checks if caller is the auction contract
        private void AssertIsAuction(){
            if (Info.Callers()[Info.Callers().Length-2] != _auctionAddress){
                Error.Throw("Only Expload auction can do that.");
            }
        }

        // // Checks if caller is owner of the specified XP asset
        private void AssertIsXPAssetOwner(long assetId){
            if (GetXPAsset(assetId).Owner != Info.Sender()){
                Error.Throw("Only owner of the asset can do that.");
            }
        }

        /*
        TradableAsset data
        */

        // Get asset class meta data using his classId
        // IMPORTANT: this method MUST be changed
        // to return valid metadata url
        public string GetClassIdMeta(Bytes classId){
            return "https://some_url/"+StdLib.BytesToHex(classId);
        }

        // Get asset instance meta data using his instanceId
        // IMPORTANT: this method MUST be changed
        // to return valid metadata url
        public string GetInstanceIdMeta(Bytes instanceId){
            return "https://some_url/"+StdLib.BytesToHex(instanceId);
        }

        // Expload's auction program address
        private Bytes _auctionAddress = new Bytes("0000000000000000000000000000000000000000000000000000000000000000");

        /// <summary>
        /// Set up Expload Auction address
        /// </summary>
        /// <param name="address"> Auction address </param>
        public void SetAuction(Bytes address){
            AssertIsGameOwner();
            _auctionAddress = address;
        }

        /*
        Interaction with the storage
        */

        /// <summary>
        /// Emit a XP asset
        /// </summary>
        /// <param name="owner"> Desired asset owner </param>
        /// <param name="classId"> Asset class id </param>
        /// <param name="instanceId"> Asset instance id </param>
        /// <returns>
        /// Emitted asset id
        /// </returns>
        public long EmitXPAsset(Bytes owner, Bytes classId, Bytes instanceId){
            // Only the game server (or owner) can emit assets
            AssertIsGameOwner();
            // Getting item's blockchain id
            var id = ++_lastXPId;
            // Parsing the asset
            var asset = new Asset(id, owner, classId, instanceId);

            // Putting the asset into storage
            _XPAssets[id] = asset;
            // Putting asset into user's storage
            var assetCount = _XPUsersAssetCount.GetOrDefault(owner, 0);
            _XPUsersAssetIds[GetUserAssetKey(owner, assetCount)] = id;
            _XPUsersAssetCount[owner] = assetCount + 1;
            _SerialNumbers[id] = assetCount;

            // Log an event
            Log.Event("EmitXP", asset);

            return id;
        }

        // Serial numbers of assets in users' asset storages
        private Mapping<long, long> _SerialNumbers =
            new Mapping<long, long>();

        /// <summary>
        /// Transfer XP asset to a new owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <param name="to"> New owner address </param>
        public void TransferXPAsset(long id, Bytes to){
            // Only the auction can transfer assets
            AssertIsAuction();
            // Passing the ownership
            var asset = GetXPAsset(id);
            var oldOwner  = asset.Owner;

            // Check if this asset actually exists
            if(oldOwner == Bytes.VOID_ADDRESS){
                Error.Throw("This asset doesn't exist.");
            }
            
            asset.Owner = to;
            _XPAssets[id] = asset;

            // Making changes to users assets storage

            // Delete from old owner's storage
            var oldOwnerAssetCount = _XPUsersAssetCount.GetOrDefault(oldOwner, 0);
            var oldOwnerSerialNumber = _SerialNumbers.GetOrDefault(id, 0);
            var lastAsset = _XPUsersAssetIds[GetUserAssetKey(oldOwner, oldOwnerAssetCount-1)];
            _XPUsersAssetIds[GetUserAssetKey(oldOwner, oldOwnerSerialNumber)] = lastAsset;
            _XPUsersAssetIds[GetUserAssetKey(oldOwner,oldOwnerAssetCount-1)] = 0;
            _XPUsersAssetCount[oldOwner] = oldOwnerAssetCount - 1;

            // Add to new owner's storage
            var newSerialNumber = _XPUsersAssetCount.GetOrDefault(to, 0);
            _XPUsersAssetIds[GetUserAssetKey(to, newSerialNumber)] = id;
            _XPUsersAssetCount[to] = newSerialNumber + 1;

            // Update asset serial number
            _SerialNumbers[id] = newSerialNumber;

            // Log an event
            Log.Event("TransferXP", asset);
        }
    }
}
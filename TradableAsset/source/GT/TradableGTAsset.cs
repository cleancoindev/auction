namespace Expload {

    using Standards;
    using Pravda;
    using System;

    [Program]
    public class TradableGTAsset : ITradableGTAsset
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

        This class represents a GT asset (can only be bought and sold for GameToken)
        
        */

        public static void Main(){ }

        // Last id given to a GT asset (id=0 is invalid)
        private long _lastGTId = 0;

        /*
        Main asset storage
        */
        
        // Mapping storing GT assets
        // This mapping's key is asset's blockchain id
        private Mapping<long, Asset> _GTAssets =
            new Mapping<long, Asset>();

        private Asset GetGTAsset(long id){
            return _GTAssets.GetOrDefault(id, new Asset());
        }

        /// <summary>
        /// Get GT asset data
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Asset object
        /// </returns>
        public Asset GetGTAssetData(long id){
            return GetGTAsset(id);
        }

        /// <summary>
        /// Get GT asset owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Owner address
        /// </returns>
        public Bytes GetGTAssetOwner(long id){
            return GetGTAsset(id).Owner;
        }

        /// <summary>
        /// Get GT asset class id
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Class id
        /// </returns>
        public Bytes GetGTAssetClassId(long id){
            return GetGTAsset(id).ItemClassId;
        }

        /*
        Users' asset storage
        */

        // Mapping storing GT assets ids belonging to a user
        // Key is the concatenation of user address and asset number in his storage
        private Mapping<string, long> _GTUsersAssetIds =
            new Mapping<string, long>();

        // Mapping storing GT user's asset counter
        private Mapping<Bytes, long> _GTUsersAssetCount =
            new Mapping<Bytes, long>();

        /// <summary>
        /// Get amount of GT assets belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// Asset amount
        /// </returns>
        public long GetUsersGTAssetCount(Bytes address){
            return _GTUsersAssetCount.GetOrDefault(address, 0);
        }

        // Get one of user's GT assets
        private long _getUsersGTAssetId(Bytes address, long number){
            // We can't get more assets than user owns
            if(number >= _GTUsersAssetCount.GetOrDefault(address, 0)){
                Error.Throw("This asset doesn't exist!");
            }
            var key = GetUserAssetKey(address, number);
            return _GTUsersAssetIds.GetOrDefault(key, 0);
        }

        /// <summary>
        /// Get asset id of a particular GT asset belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <param name="number"> Asset serial number </param>
        /// <returns>
        /// Asset id
        /// </returns>
        public long GetUsersGTAssetId(Bytes address, long number){
            return _getUsersGTAssetId(address, number);
        }

        /// <summary>
        /// Get list of GT assets
        /// belonging to a particular user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// List of asset objects
        /// </returns>
        public Asset[] GetUsersAllGTAssetsData(Bytes address){
            int amount = (int)_GTUsersAssetCount.GetOrDefault(address, 0);
            var result = new Asset[amount];
            for(int num = 0; num < amount; num++){
                result[num] = GetGTAsset(_getUsersGTAssetId(address, num));
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


        // // Checks if caller is owner of the specified GT asset
        private void AssertIsGTAssetOwner(long assetId){
            if (GetGTAsset(assetId).Owner != Info.Sender()){
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
        /// Emit a GT asset
        /// </summary>
        /// <param name="owner"> Desired asset owner </param>
        /// <param name="classId"> Asset class id </param>
        /// <param name="instanceId"> Asset instance id </param>
        /// <returns>
        /// Emitted asset id
        /// </returns>
        public long EmitGTAsset(Bytes owner, Bytes classId, Bytes instanceId){
            // Only the game server (or owner) can emit assets
            AssertIsGameOwner();
            // Getting item's blockchain id
            var id = ++_lastGTId;
            // Parsing the asset
            var asset = new Asset(id, owner, classId, instanceId);

            // Putting the asset into storage
            _GTAssets[id] = asset;
            // Putting asset into user's storage
            var assetCount = _GTUsersAssetCount.GetOrDefault(owner, 0);
            var key = GetUserAssetKey(owner, assetCount);
            _GTUsersAssetIds[key] = id;
            _GTUsersAssetCount[owner] = assetCount + 1;
            _SerialNumbers[id] = assetCount;

            // Log an event
            Log.Event("EmitGT", asset);

            return id;
        }

        // Serial numbers of assets in users' asset storages
        private Mapping<long, long> _SerialNumbers =
            new Mapping<long, long>();

        /// <summary>
        /// Transfer GT asset to a new owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <param name="to"> New owner address </param>
        public void TransferGTAsset(long id, Bytes to){
            // Only the auction can transfer assets
            AssertIsAuction();
            // Passing the ownership
            var asset = GetGTAsset(id);
            var oldOwner  = asset.Owner;

            // Check if this asset actually exists
            if(oldOwner == Bytes.VOID_ADDRESS){
                Error.Throw("This asset doesn't exist.");
            }

            asset.Owner = to;
            _GTAssets[id] = asset;

            // Making changes to users assets storage

            // Delete from old owner's storage
            var oldOwnerAssetCount = _GTUsersAssetCount.GetOrDefault(oldOwner, 0);
            var oldOwnerSerialNumber = _SerialNumbers.GetOrDefault(id, 0);
            var lastAsset = _GTUsersAssetIds.GetOrDefault(GetUserAssetKey(oldOwner, oldOwnerAssetCount-1), 0);
            _GTUsersAssetIds[GetUserAssetKey(oldOwner, oldOwnerSerialNumber)] = lastAsset;
            _GTUsersAssetIds[GetUserAssetKey(oldOwner,oldOwnerAssetCount-1)] = 0;
            _GTUsersAssetCount[oldOwner] = oldOwnerAssetCount - 1;

            // Add to new owner's storage
            var assetCount = _GTUsersAssetCount.GetOrDefault(to, 0);
            var key = GetUserAssetKey(to, assetCount);
            _GTUsersAssetIds[key] = id;
            _GTUsersAssetCount[to] = assetCount + 1;

            // Log an event
            Log.Event("TransferGT", asset);
        }
    }
}
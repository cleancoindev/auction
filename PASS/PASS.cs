namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class PASS {
        /*
        This program defines a common standart
        for storing game assets in Pravda blockchain
        and interacting with them.

        The pattern should be followed for Expload Platform
        to be able to interact with game contracts,
        for example, in order to use Expload assets auction.

        ---------------------------------

        Each asset has three types of IDs:

        ---------------------------------

        Blockchain id - used to navigate storage mappings in
        PASS contract, has no in-game meaning
        Can't equal 0 (0 means no asset exists)

        ---------------------------------

        External game id - the id which shows what particular
        in-game item is behind the asset stored in the contract

        ---------------------------------

        Meta id - id of object meta-data (name, description, ...),
        getMeta(metaId) should return a link to JSON file:

        {
            "name": <itemName>,
            "desc": <itemShortDescription>,
            "pic": <itemPictureURL>,
            "misc": <miscData*>
        }

        *miscData - full item description, item stats, etc.

        ---------------------------------

        Also the storage and methods are split into two groups:
        For interacting with GT (GameToken) assets,
        For interacting with XC (XCoin) assets.

        Assets bought for GT can't be sold on XC auction, and vice-versa,
        only GT methods should be use for GT assets, similarly for XC assets.
        */

        public static void Main(){ }

        // Dump Asset into JSON
        private string DumpAsset(Asset asset){
            return
            "{" +
                "\"id\": \""         + System.Convert.ToString(asset.id)   + "\"," +
                "\"owner\": \""      + StdLib.BytesToHex(asset.owner)      + "\"," +
                "\"externalId\": \"" + StdLib.BytesToHex(asset.externalId) + "\"," +
                "\"metaId\": \""     + getMetaData(asset.metaId)           + "\""  +
            "}";
        }

        // Last id given to a GT asset (id=0 is invalid)
        private UInt32 lastGTId = 0;

        // Last id given to an XC asset (id=0 is invalid)
        private UInt32 lastXCId = 0;

        /*
        Main asset storage
        */
        
        // Mapping storing GT assets
        // This mapping's key is asset's blockchain id
        private Mapping<UInt32, Asset> GTAssets =
            new Mapping<UInt32, Asset>();

        private Asset getGTAsset(UInt32 id){
            return GTAssets.GetOrDefault(id, new Asset());
        }

        public string getGTAssetData(UInt32 id){
            return DumpAsset(getGTAsset(id));
        }

        public Bytes getGTAssetOwner(UInt32 id){
            return getGTAsset(id).owner;
        }

        public Bytes getGTAssetExternalId(UInt32 id){
            return getGTAsset(id).externalId;
        }

        // Mapping storing XC assets
        // This mapping's key is asset's blockchain id
        private Mapping<UInt32, Asset> XCAssets =
            new Mapping<UInt32, Asset>();

        private Asset getXCAsset(UInt32 id){
            return XCAssets.GetOrDefault(id, new Asset());
        }

        public string getXCAssetData(UInt32 id){
            return DumpAsset(getXCAsset(id));
        }

        public Bytes getXCAssetOwner(UInt32 id){
            return getXCAsset(id).owner;
        }

        public Bytes getXCAssetExternalId(UInt32 id){
            return getXCAsset(id).externalId;
        }

        /*
        Users' asset storage
        */

        // Mapping storing GT assets ids belonging to a user
        // Key is the concatenation of user address and asset number in his storage
        private Mapping<string, UInt32> GTUsersAssetIds =
            new Mapping<string, UInt32>();

        // Mapping storing GT user's asset counter
        private Mapping<Bytes, UInt32> GTUsersAssetCount =
            new Mapping<Bytes, UInt32>();

        // Get user's asset counter
        public UInt32 getGTUsersAssetCount(Bytes address){
            return GTUsersAssetCount.GetOrDefault(address, 0);
        }

        // Get one of user's GT assets
        private UInt32 _getUsersGTAssetId(Bytes address, UInt32 number){
            // We can't get more assets than user owns
            if(number >= GTUsersAssetCount.GetOrDefault(address, 0)){
                Error.Throw("This asset doesn't exist!");
            }
            string key = getUserAssetKey(address, number);
            return GTUsersAssetIds.GetOrDefault(key, 0);
        }

        public UInt32 getUsersGTAssetId(Bytes address, UInt32 number){
            return _getUsersGTAssetId(address, number);
        }

        // Get all of user's GT assets data JSONified
        public string getUsersAllGTAssetsData(Bytes address){
            string result = "[";
            UInt32 amount = GTUsersAssetCount.GetOrDefault(address, 0);
            for(UInt32 num = 0; num < amount; num++){
                result += DumpAsset(getGTAsset(_getUsersGTAssetId(address, num)));
                if(num < amount - 1){
                    result += ",";
                }
            }
            return result + "]";
        }

        // Mapping storing XC assets ids belonging to a user
        private Mapping<string, UInt32> XCUsersAssetIds =
            new Mapping<string, UInt32>();

        // Mapping storing XC user's asset counter
        private Mapping<Bytes, UInt32> XCUsersAssetCount =
            new Mapping<Bytes, UInt32>();

        // Get user's asset counter
        public UInt32 getXCUsersAssetCount(Bytes address){
            return XCUsersAssetCount.GetOrDefault(address, 0);
        }

        // Get one of user's XC assets
        private UInt32 _getUsersXCAssetId(Bytes address, UInt32 number){
            // We can't get more assets than user owns
            if(number >= XCUsersAssetCount.GetOrDefault(address, 0)){
                Error.Throw("This asset doesn't exist!");
            }
            string key = getUserAssetKey(address, number);
            return XCUsersAssetIds.GetOrDefault(key, 0);
        }

        public UInt32 getUsersXCAssetId(Bytes address, UInt32 number){
            return _getUsersXCAssetId(address, number);
        }

        // Get all of user's XC assets data JSONified
        public string getUsersAllXCAssetsData(Bytes address){
            string result = "[";
            UInt32 amount = XCUsersAssetCount.GetOrDefault(address, 0);
            for(UInt32 num = 0; num < amount; num++){
                result += DumpAsset(getXCAsset(_getUsersXCAssetId(address, num)));
                if(num < amount - 1){
                    result += ",";
                }
            }
            return result + "]";
        }

        // Get key for users asset storage
        private string getUserAssetKey(Bytes address, UInt32 number){
            return (StdLib.BytesToHex(address) + System.Convert.ToString(number));
        }

        /*
        Permission-checkers
        */

        // Checks if caller is the owner of the contract
        // (if it's a call from game's server)
        private void assertIsGameOwner(){
            if (Info.Sender() != Info.ProgramAddress()){
                Error.Throw("Only owner of the program can do that.");
            }
        }

        // Checks if caller is the auction contract
        private void assertIsAuction(){
            if (Info.Callers()[Info.Callers().Length-2] != auctionAddress){
                Error.Throw("Only Expload auction can do that.");
            }
        }


        // // Checks if caller is owner of the specified GT asset
        private void assertIsGTAssetOwner(UInt32 assetId){
            if (getGTAsset(assetId).owner != Info.Sender()){
                Error.Throw("Only owner of the asset can do that.");
            }
        }

        // // Checks if caller is owner of the specified XC asset
        private void assertIsXCAssetOwner(UInt32 assetId){
            if (getXCAsset(assetId).owner != Info.Sender()){
                Error.Throw("Only owner of the asset can do that.");
            }
        }

        /*
        PASS data
        */

        // Get asset meta data using his metaId
        // IMPORTANT: this method MUST be changed
        // to return valid metadata url
        private string getMetaData(Bytes metaId){
            return "https://some_url/"+StdLib.BytesToHex(metaId);
        }

        // Expload's auction smart contract address
        private Bytes auctionAddress = new Bytes("0000000000000000000000000000000000000000000000000000000000000000");

        // Setting up auction address
        public void SetAuction(Bytes addr){
            assertIsGameOwner();
            auctionAddress = addr;
        }

        /*
        Interaction with the storage
        */

        // Interaction with GT assets storage

        public UInt32 EmitGTAsset(Bytes owner, Bytes externalId, Bytes metaId){
            // Only the gameserver (or owner) can emit assets
            assertIsGameOwner();
            // Getting item's blockchain id
            UInt32 id = ++lastGTId;
            // Parsing the asset
            Asset asset = new Asset(id, owner, externalId, metaId);

            // Putting the asset into storage
            GTAssets[id] = asset;
            // Putting asset into user's storage
            UInt32 assetCount = GTUsersAssetCount.GetOrDefault(owner, 0);
            string key = getUserAssetKey(owner, assetCount);
            GTUsersAssetIds[key] = id;
            GTUsersAssetCount[owner] = assetCount + 1;

            // Log an event
            Log.Event("EmitGT", DumpAsset(asset));

            return id;
        }

        public void TransferGTAsset(UInt32 id, Bytes to){
            // Only the auction can transfer assets
            assertIsAuction();
            // Passing the ownership
            Asset asset = getGTAsset(id);
            Bytes oldOwner  = asset.owner;

            // Check if this asset actually exists
            if(oldOwner == Bytes.VOID_ADDRESS){
                Error.Throw("This asset doesn't exist.");
            }

            asset.owner = to;
            GTAssets[id] = asset;

            // Making changes to users assets storage

            // Delete from old owner's storage
            UInt32 oldOwnerassetCount = GTUsersAssetCount.GetOrDefault(oldOwner, 0);
            for(UInt32 i = 0; i < oldOwnerassetCount; i++){
                if(GTUsersAssetIds.GetOrDefault(getUserAssetKey(oldOwner, i), 0) == id){
                    UInt32 lastAsset = GTUsersAssetIds.GetOrDefault(getUserAssetKey(oldOwner, oldOwnerassetCount-1), 0);
                    GTUsersAssetIds[getUserAssetKey(oldOwner, i)] = lastAsset;
                    GTUsersAssetIds[getUserAssetKey(oldOwner,oldOwnerassetCount-1)] = 0;
                    GTUsersAssetCount[oldOwner] = oldOwnerassetCount - 1;
                    break;
                }
            }

            // Add to new onwer's storage
            UInt32 assetCount = GTUsersAssetCount.GetOrDefault(to, 0);
            string key = getUserAssetKey(to, assetCount);
            GTUsersAssetIds[key] = id;
            GTUsersAssetCount[to] = assetCount + 1;

            // Log an event
            Log.Event("TransferGT", DumpAsset(asset));
        }

        // Interaction with XC assets storage

        public UInt32 EmitXCAsset(Bytes owner, Bytes externalId, Bytes metaId){
            // Only the gameserver (or owner) can emit assets
            assertIsGameOwner();
            // Getting item's blockchain id
            UInt32 id = ++lastXCId;
            // Parsing the asset
            Asset asset = new Asset(id, owner, externalId, metaId);

            // Putting the asset into storage
            XCAssets[id] = asset;
            // Putting asset into user's storage
            UInt32 assetCount = XCUsersAssetCount.GetOrDefault(owner, 0);
            string key = getUserAssetKey(owner, assetCount);
            XCUsersAssetIds[key] = id;
            XCUsersAssetCount[owner] = assetCount + 1;

            // Log an event
            Log.Event("EmitXC", DumpAsset(asset));

            return id;
        }

        public void TransferXCAsset(UInt32 id, Bytes to){
            // Only the auction can transfer assets
            assertIsAuction();
            // Passing the ownership
            Asset asset = getXCAsset(id);
            Bytes oldOwner  = asset.owner;

            // Check if this asset actually exists
            if(oldOwner == Bytes.VOID_ADDRESS){
                Error.Throw("This asset doesn't exist.");
            }
            
            asset.owner = to;
            XCAssets[id] = asset;

            // Making changes to users assets storage

            // Delete from old owner's storage
            UInt32 oldOwnerassetCount = XCUsersAssetCount.GetOrDefault(oldOwner, 0);
            for(UInt32 i = 0; i < oldOwnerassetCount; i++){
                if(XCUsersAssetIds.GetOrDefault(getUserAssetKey(oldOwner, i), 0) == id){
                    UInt32 lastAsset = XCUsersAssetIds.GetOrDefault(getUserAssetKey(oldOwner, oldOwnerassetCount-1), 0);
                    XCUsersAssetIds[getUserAssetKey(oldOwner, i)] = lastAsset;
                    XCUsersAssetIds[getUserAssetKey(oldOwner,oldOwnerassetCount-1)] = 0;
                    XCUsersAssetCount[oldOwner] = oldOwnerassetCount - 1;
                    break;
                }
            }

            // Add to new onwer's storage
            UInt32 assetCount = XCUsersAssetCount.GetOrDefault(to, 0);
            string key = getUserAssetKey(to, assetCount);
            XCUsersAssetIds[key] = id;
            XCUsersAssetCount[to] = assetCount + 1;

            // Log an event
            Log.Event("TransferXC", DumpAsset(asset));
        }
    }

    public class Asset {
        /*
        Class defining a game asset
        */
        
        public Asset(UInt32 id, Bytes owner, Bytes externalId, Bytes metaId){
            this.id = id;
            this.owner = owner;
            this.externalId = externalId;
            this.metaId = metaId;
        }
        
        public Asset() { }

        // Asset's blockchain id
        public UInt32 id { get; set; } = 0;

        // Adress of asset's owner
        public Bytes owner { get; set; } = Bytes.VOID_ADDRESS;

        // Game's external asset id
        // E.g. two identical in-game swords
        // Have same internal game id
        public Bytes externalId { get; set; } = Bytes.VOID_ADDRESS;

        // External meta-data identifier
        public Bytes metaId { get; set; } = Bytes.VOID_ADDRESS;
    }

}
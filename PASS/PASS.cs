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

        // Parse arguments into Asset object
        private Asset ParseAsset(Bytes owner, Bytes externalId, Bytes metaId){
            var asset = new Asset();
            asset.owner = owner;
            asset.externalId = externalId;
            asset.metaId = metaId;
            return asset;
        }

        // Dump Asset into JSON
        private string DumpAsset(Asset asset){
            return
            "{" +
                "\"owner\": \""      + BytesToHex(asset.owner)      + "\"," +
                "\"externalId\": \"" + BytesToHex(asset.externalId) + "\"," +
                "\"metaId\": \""     + getMetaData(asset.metaId)    + "\""  +
            "}";
        }

        // Last id given to a GT asset (id=0 is invalid)
        public UInt32 lastGTId = 0;

        // Last id given to an XC asset (id=0 is invalid)
        public UInt32 lastXCId = 0;

        // Mapping storing GT assets
        // This mapping's key is asset's blockchain id
        public Mapping<UInt32, Asset> GTAssets =
            new Mapping<UInt32, Asset>();

        private Asset getGTAsset(UInt32 id){
            return GTAssets.getDefault(id, new Asset());
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
        public Mapping<UInt32, Asset> XCAssets =
            new Mapping<UInt32, Asset>();

        private Asset getXCAsset(UInt32 id){
            return XCAssets.getDefault(id, new Asset());
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

        // Mapping storing GT assets ids belonging to a user
        // Key is the concatenation of user address and asset number in his storage
        public Mapping<string, UInt32> GTUsersAssetIds =
            new Mapping<string, UInt32>();

        // Mapping storing GT user's asset counter
        public Mapping<Bytes, UInt32> GTUsersAssetCount =
            new Mapping<Bytes, UInt32>();

        // Get user's asset counter
        public UInt32 getGTUsersAssetCount(Bytes address){
            return GTUsersAssetCount.getDefault(address, 0);
        }

        // Get one of user's GT assets
        private UInt32 _getUsersGTAssetId(Bytes address, UInt32 number){
            // We can't get more assets than user owns
            if(number >= GTUsersAssetCount.getDefault(address, 0)){
                Error.Throw("This asset doesn't exist!");
            }
            string key = getUserAssetKey(address, number);
            return GTUsersAssetIds.get(key);
        }

        public UInt32 getUsersGTAssetId(Bytes address, UInt32 number){
            return _getUsersGTAssetId(address, number);
        }

        // Get all of user's GT assets data
        public string getUsersAllGTAssetsData(Bytes address){
            string result = "[";
            UInt32 amount = GTUsersAssetCount.getDefault(address, 0);
            for(UInt32 num = 0; num < amount; num++){
                result += DumpAsset(getGTAsset(_getUsersGTAssetId(address, num)));
                if(num < amount - 1){
                    result += ",";
                }
            }
            return result + "]";
        }

        // Mapping storing XC assets ids belonging to a user
        public Mapping<string, UInt32> XCUsersAssetIds =
            new Mapping<string, UInt32>();

        // Mapping storing XC user's asset counter
        public Mapping<Bytes, UInt32> XCUsersAssetCount =
            new Mapping<Bytes, UInt32>();

        // Get user's asset counter
        public UInt32 getXCUsersAssetCount(Bytes address){
            return XCUsersAssetCount.getDefault(address, 0);
        }

        // Get one of user's XC assets
        private UInt32 _getUsersXCAssetId(Bytes address, UInt32 number){
            // We can't get more assets than user owns
            if(number >= XCUsersAssetCount.getDefault(address, 0)){
                Error.Throw("This asset doesn't exist!");
            }
            string key = getUserAssetKey(address, number);
            return XCUsersAssetIds.get(key);
        }

        public UInt32 getUsersXCAssetId(Bytes address, UInt32 number){
            return _getUsersXCAssetId(address, number);
        }

        // Get all of user's XC assets data
        public string getUsersAllXCAssetsData(Bytes address){
            string result = "[";
            UInt32 amount = XCUsersAssetCount.getDefault(address, 0);
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
            return (BytesToHex(address) + System.Convert.ToString(number));
        }

        // Get asset meta data using his metaId
        // IMPORTANT: this method MUST be changed
        // to return valid metadata url
        private string getMetaData(Bytes metaId){
            return "https://some_url/"+BytesToHex(metaId);
        }

        // Expload's auction smart contract address
        Bytes auctionAddress = new Bytes("0000000000000000000000000000000000000000000000000000000000000000");

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
        Interaction with the storage
        */

        // Setting up auction address
        public void SetAuction(Bytes addr){
            assertIsGameOwner();
            auctionAddress = addr;
        }

        // Interaction with GT assets storage

        public UInt32 EmitGTAsset(Bytes owner, Bytes externalId, Bytes metaId){
            // Only the gameserver (or owner) can emit assets
            assertIsGameOwner();
            // Parsing the asset
            Asset asset = ParseAsset(owner, externalId, metaId);
            // Getting item's blockchain id
            UInt32 id = ++lastGTId;

            // Putting the asset into storage
            GTAssets.put(id, asset);
            // Putting asset into user's storage
            UInt32 assetCount = GTUsersAssetCount.getDefault(owner, 0);
            string key = getUserAssetKey(owner, assetCount);
            GTUsersAssetIds.put(key, id);
            GTUsersAssetCount.put(owner, assetCount + 1);

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
            GTAssets.put(id, asset);

            /*
            Making changes to users assets storage
            */ 

            // Delete from old owner's storage
            UInt32 oldOwnerassetCount = GTUsersAssetCount.getDefault(oldOwner, 0);
            for(UInt32 i = 0; i < oldOwnerassetCount; i++){
                if(GTUsersAssetIds.get(getUserAssetKey(oldOwner, i)) == id){
                    UInt32 lastAsset = GTUsersAssetIds.get(getUserAssetKey(oldOwner, oldOwnerassetCount-1));
                    GTUsersAssetIds.put(getUserAssetKey(oldOwner, i), lastAsset);
                    GTUsersAssetIds.put(getUserAssetKey(oldOwner,oldOwnerassetCount-1), 0);
                    GTUsersAssetCount.put(oldOwner, oldOwnerassetCount - 1);
                    break;
                }
            }

            // Add to new onwer's storage
            UInt32 assetCount = GTUsersAssetCount.getDefault(to, 0);
            string key = getUserAssetKey(to, assetCount);
            GTUsersAssetIds.put(key, id);
            GTUsersAssetCount.put(to, assetCount + 1);

            // Log an event
            Log.Event("TransferGT", DumpAsset(asset));
        }

        // Interaction with XC assets storage

        public UInt32 EmitXCAsset(Bytes owner, Bytes externalId, Bytes metaId){
            // Only the gameserver (or owner) can emit assets
            assertIsGameOwner();
            // Parsing the asset
            Asset asset = ParseAsset(owner, externalId, metaId);
            // Getting item's blockchain id
            UInt32 id = ++lastXCId;

            // Putting the asset into storage
            XCAssets.put(id, asset);
            // Putting asset into user's storage
            UInt32 assetCount = XCUsersAssetCount.getDefault(owner, 0);
            string key = getUserAssetKey(owner, assetCount);
            XCUsersAssetIds.put(key, id);
            XCUsersAssetCount.put(owner, assetCount + 1);

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
            XCAssets.put(id, asset);

            /*
            Making changes to users assets storage
            */ 

            // Delete from old owner's storage
            UInt32 oldOwnerassetCount = XCUsersAssetCount.getDefault(oldOwner, 0);
            for(UInt32 i = 0; i < oldOwnerassetCount; i++){
                if(XCUsersAssetIds.get(getUserAssetKey(oldOwner, i)) == id){
                    UInt32 lastAsset = XCUsersAssetIds.get(getUserAssetKey(oldOwner, oldOwnerassetCount-1));
                    XCUsersAssetIds.put(getUserAssetKey(oldOwner, i), lastAsset);
                    XCUsersAssetIds.put(getUserAssetKey(oldOwner,oldOwnerassetCount-1), 0);
                    XCUsersAssetCount.put(oldOwner, oldOwnerassetCount - 1);
                    break;
                }
            }

            // Add to new onwer's storage
            UInt32 assetCount = XCUsersAssetCount.getDefault(to, 0);
            string key = getUserAssetKey(to, assetCount);
            XCUsersAssetIds.put(key, id);
            XCUsersAssetCount.put(to, assetCount + 1);

            // Log an event
            Log.Event("TransferXC", DumpAsset(asset));
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

        private string ByteToHex(byte b){
            return HexPart(b / 16) + HexPart(b % 16);
        }

        private string BytesToHex(Bytes bytes){
            string res = "";
            for (int i = 0; i < bytes.Length(); i++){
                res += ByteToHex(bytes[i]);
            }
            return res;
        }
    }

    public class Asset {
        /*
        Class defining a game asset
        */

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
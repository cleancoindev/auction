using Expload.Pravda;
using System;

namespace PcallNamespace {

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

        public static void Main() { }

        // Last id given to a GT asset
        public UInt32 lastGTId = 0;

        // Last id given to an XC asset
        public UInt32 lastXCId = 0;

        // Mapping storing GT assets
        // This mapping's key is asset's blockchain id
        public Mapping<UInt32, Asset> GTAssets =
            new Mapping<UInt32, Asset>();

        public Asset getGTAsset(UInt32 id){
            return GTAssets.getDefault(id, new Asset());
        }

        // Mapping storing XC assets
        // This mapping's key is asset's blockchain id
        public Mapping<UInt32, Asset> XCAssets =
            new Mapping<UInt32, Asset>();

        public Asset getXCAsset(UInt32 id){
            return XCAssets.getDefault(id, new Asset());
        }

        // Get asset meta data using his metaId
        public string getMeta(Bytes metaId){
            return "some_url";
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
            if (Info.Callers()[-2] != auctionAddress){
                Error.Throw("Only Expload auction can do that.");
            }
        }

        // Checks if caller is owner of the specified GT asset
        private void assertIsGTAssetOwner(UInt32 assetId){
            if (getGTAsset(assetId).owner != Info.Sender()){
                Error.Throw("Only owner of the asset can do that.");
            }
        }

        // Checks if caller is owner of the specified XC asset
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

        public UInt32 EmitGTAsset(Asset asset, Bytes owner){
            // Only the gameserver (or owner) can emit assets
            assertIsGameOwner();
            // Getting item's blockchain id
            UInt32 id = ++lastGTId;

            // Putting the asset into storage
            GTAssets.put(id, asset);

            return id;
        }

        public void TransferGTAsset(UInt32 id, Bytes to){
            // Only the auction can transfer assets
            assertIsAuction();
            // Passing the ownership
            getGTAsset(id).owner = to;
        }

        // Interaction with GT assets storage

        public UInt32 EmitXCAsset(Asset asset, Bytes owner){
            // Only the gameserver (or owner) can emit assets
            assertIsGameOwner();
            // Getting item's blockchain id
            UInt32 id = ++lastXCId;

            // Putting the asset into storage
            XCAssets.put(id, asset);

            return id;
        }

        public void TransferXCAsset(UInt32 id, Bytes to){
            // Only the auction can transfer assets
            assertIsAuction();
            // Passing the ownership
            getXCAsset(id).owner = to;
        }
    }

    public class Asset {
        /*
        Class defining a game asset
        */

        // Adress of asset's owner
        public Bytes owner;

        // Game's external asset id
        // E.g. two identical in-game swords
        // Have same internal game id
        public Bytes externalId;

        // External meta-data identifier
        public Bytes metaId;
    }
}
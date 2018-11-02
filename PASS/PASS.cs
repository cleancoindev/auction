namespace Expload {

    using Pravda;
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

        Each asset has two types of IDs:

        Blockchain id - used to navigate storage mappings in
        PASS contract, has no in-game meaning

        Internal game id - the id which shows what particular
        in-game item is behind the asset stored in the contract
        */
        public static void Main() { }

        // Last id given to an asset
        public UInt64 lastId;

        /*
        As Pravda blockchain doesn't currently support
        Objects storage, the mappings below are used
        To store class fields of asset objects
        */

        // Mapping storing the owners of assets
        public Mapping<UInt64, Bytes> Owners =
            new Mapping<UInt64, Bytes>();

        // Mapping storing the assets' game ids
        public Mapping<UInt64, UInt32> GameIds =
            new Mapping<UInt64, UInt32>();

        // Mapping storing assets' sellability types
        // (If they can be sold for XCoin or not)
        public Mapping<UInt64, bool> Sellability =
            new Mapping<UInt64, bool>();

        // Mapping storing assets' in-game names
        public Mapping<UInt64, string> ItemNames =
            new Mapping<UInt64, string>();

        // Mapping storing assets' in-game descriptions
        public Mapping<UInt64, string> ItemDescs =
            new Mapping<UInt64, string>();

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

        // Checks if caller is owner of the specified asset
        private void assertIsAssetOwner(UInt64 assetId){
            if (Owners.getDefault(assetId, Bytes.EMPTY) != Info.Sender()){
                Error.Throw("Only owner of the asset can do that.");
            }
        }

        /*
        Interaction with the storage
        */

        public UInt64 EmitAsset(Asset asset, Bytes owner){
            // Only the gameserver (or owner) can emit assets
            assertIsGameOwner();
            // Getting item's blockchain id
            var id = lastId++;

            // Putting all assets's class fields
            // into the storage
            Owners.put(id, owner);
            GameIds.put(id, asset.gameId);
            Sellability.put(id, asset.XCoinSellable);
            ItemNames.put(id, asset.ItemName);
            ItemDescs.put(id, asset.ItemDesc);

            return id;
        }

        public UInt64 EmitAsset(UInt32 gameId, bool XCoinSellable,
            string ItemName, string ItemDesc, Bytes owner){
            // Only the gameserver (or owner) can emit assets
            assertIsGameOwner();
            // Getting item's blockchain id
            var id = lastId++;

            // Putting all assets's class fields
            // into the storage
            Owners.put(id, owner);
            GameIds.put(id, gameId);
            Sellability.put(id, XCoinSellable);
            ItemNames.put(id, ItemName);
            ItemDescs.put(id, ItemDesc);

            return id;
        }

        public void TransferAsset(UInt64 id, Bytes to){
            // Only the asset owner can give it
            // to someone else
            assertIsAssetOwner(id);

            // Passing the ownership
            Owners.put(id, to);
        }
    }

    public class Asset {
        /*
        Class defining a common game asset
        */

        // Blockchain asset id
        // E.g. two identical in-game swords
        // Have different blockchain id
        public UInt64 id;

        // Game's internal asset id
        // E.g. two identical in-game swords
        // Have same internal game id
        public UInt32 gameId;

        // Adress of asset's owner
        public Bytes owner;

        // Asset auction accessebility type:
        // 1 - may be sold for XCoin
        // 0 - may be sold for GameToken only
        public bool XCoinSellable;

        // Asset's metadata for UI

        // Asset's in-game name
        public string ItemName;
        // Asset's description
        public string ItemDesc;
    }

    public class Lot {
        /*
        Class defining auction lot
        */

        // The asset to be sold
        public Asset item;

        // Lot's starting price
        public UInt32 startingPrice;

        // Current highest bid
        public UInt32 lastBid;

        // Current highest bidder
        public Bytes bidder;
    }
}
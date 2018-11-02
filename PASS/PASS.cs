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
        public object meta = new {
            // Asset's in-game name
            ItemName = (string) null,
            // Asset's description
            ItemDesc = (string) null,
        };
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
namespace Expload.Standards {

    using Pravda;
    using System;

    [Program]
    public class Emit {
        public static void Main() { }

        public Asset test_Emit() {
            // Init addresses and get program by address
            var programOwner = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            var assetOwner   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            // Emit the asset
            var externalId = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            var metaId = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            var assetId = ProgramHelper.Program<TradableGTAsset>(programOwner).EmitGTAsset(
                assetOwner, externalId, metaId
            );

            // Get asset data
            var assetData = ProgramHelper.Program<TradableGTAsset>(programOwner).GetGTAssetData(assetId);

            // Return asset object
            return assetData;
        }
    }
}
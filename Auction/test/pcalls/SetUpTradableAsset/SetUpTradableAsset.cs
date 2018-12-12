namespace Expload {

    using Pravda;
    using Standarts;
    using System;

    [Program]
    public class SetUpTradableAsset {
        public static void Main() { }

        public void test_SetUpTradableAsset() {
            // Init addresses and get program by address
            var tradableAssetAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            var auctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");
            var userAddress = new Bytes("8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f");

            // Emit the asset
            var externalId1 = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            var metaId1 = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            var assetId1 = ProgramHelper.Program<TradableAsset>(tradableAssetAddress).EmitXCAsset(
                userAddress, externalId1, metaId1
            );

            // Check if asset was emitted
            if(userAddress != ProgramHelper.Program<TradableAsset>(tradableAssetAddress).GetXCAssetOwner(1)){
                Error.Throw("Asset 1 was not emitted.");
            }

            // Emit the second asset
            var externalId2 = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            var metaId2 = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            var assetId2 = ProgramHelper.Program<TradableAsset>(tradableAssetAddress).EmitXCAsset(
                userAddress, externalId2, metaId2
            );

            // Check if asset was emitted
            if(userAddress != ProgramHelper.Program<TradableAsset>(tradableAssetAddress).GetXCAssetOwner(2)){
                Error.Throw("Asset 2 was not emitted.");
            }

            // Set auction address
            ProgramHelper.Program<TradableAsset>(tradableAssetAddress).SetAuction(auctionAddress);
        }
    }
}
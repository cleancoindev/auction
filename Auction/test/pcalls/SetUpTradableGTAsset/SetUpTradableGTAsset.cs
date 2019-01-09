namespace Expload {

    using Pravda;
    using Standards;
    using System;

    [Program]
    public class SetUpTradableGTAsset {
        public static void Main() { }

        public void test_SetUpTradableGTAsset() {
            // Init addresses and get program by address
            var tradableGTAssetAddress = new Bytes("17e22f66979eca19a8b060a8bb759bfb3dbbce785a039e9e1ed01a54cc92161c");
            var auctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");
            var userAddress = new Bytes("8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f");

            // Emit the asset
            var externalId1 = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            var metaId1 = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            var assetId1 = ProgramHelper.Program<TradableGTAsset>(tradableGTAssetAddress).EmitGTAsset(
                userAddress, externalId1, metaId1
            );

            // Check if asset was emitted
            if(userAddress != ProgramHelper.Program<TradableGTAsset>(tradableGTAssetAddress).GetGTAssetOwner(assetId1)){
                Error.Throw("Asset 1 was not emitted.");
            }

            // Emit the second asset
            var externalId2 = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            var metaId2 = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            var assetId2 = ProgramHelper.Program<TradableGTAsset>(tradableGTAssetAddress).EmitGTAsset(
                userAddress, externalId2, metaId2
            );

            // Check if asset was emitted
            if(userAddress != ProgramHelper.Program<TradableGTAsset>(tradableGTAssetAddress).GetGTAssetOwner(assetId2)){
                Error.Throw("Asset 2 was not emitted.");
            }

            // Set auction address
            ProgramHelper.Program<TradableGTAsset>(tradableGTAssetAddress).SetAuction(auctionAddress);
        }
    }
}
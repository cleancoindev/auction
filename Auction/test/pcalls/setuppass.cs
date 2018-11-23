namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class setuppass {
        public static void Main() { }

        public void test_setuppass() {
            // Init addresses and get program by address
            Bytes PassAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes AuctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");
            Bytes userAddress = new Bytes("8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f");

            // Emit the asset
            Bytes externalId_1 = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            Bytes metaId_1 = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            UInt32 assetId_1 = ProgramHelper.Program<PASS>(PassAddress).EmitXCAsset(
                userAddress, externalId_1, metaId_1
            );

            // Check if asset was emitted
            if(userAddress != ProgramHelper.Program<PASS>(PassAddress).getXCAssetOwner(1)){
                Error.Throw("Asset 1 was not emitted.");
            }

            // Emit the second asset
            Bytes externalId_2 = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            Bytes metaId_2 = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            UInt32 assetId_2 = ProgramHelper.Program<PASS>(PassAddress).EmitXCAsset(
                userAddress, externalId_2, metaId_2
            );

            // Check if asset was emitted
            if(userAddress != ProgramHelper.Program<PASS>(PassAddress).getXCAssetOwner(2)){
                Error.Throw("Asset 2 was not emitted.");
            }

            // Set auction address
            ProgramHelper.Program<PASS>(PassAddress).SetAuction(AuctionAddress);
        }
    }
}
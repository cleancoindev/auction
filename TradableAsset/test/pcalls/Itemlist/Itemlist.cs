namespace Expload.Standarts {

    using Pravda;
    using System;

    [Program]
    public class Itemlist {
        public static void Main() { }

        public void test_Itemlist() {
            // Init addresses and get program by address
            Bytes programOwner = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes assetOwner   = new Bytes("8743e40cd8a5e162272fc8a5c56595b3aa9abb9708f26abc88f1d61cbb5576dc");

            // Emit the asset
            Bytes externalId = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            Bytes metaId = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            long assetId1 = ProgramHelper.Program<TradableAsset>(programOwner).EmitGTAsset(
                assetOwner, externalId, metaId
            );

            // Emit one more asset
            long assetId2 = ProgramHelper.Program<TradableAsset>(programOwner).EmitGTAsset(
                assetOwner, externalId, metaId
            );

            // Get asset counter
            long assetCounter = ProgramHelper.Program<TradableAsset>(programOwner).GetGTUsersAssetCount(assetOwner);

            if(assetCounter != 2) {
                Error.Throw("Wrong amount of assets emitted!");
            }

            // Get id of first emitted asset
            long actualAssetId1 = ProgramHelper.Program<TradableAsset>(programOwner).GetUsersGTAssetId(assetOwner, 0);

            if(actualAssetId1 != assetId1) {
                Error.Throw("Wrong asset written to user asset list!");
            }

            // Get id of second emitted asset
            long actualAssetId2 = ProgramHelper.Program<TradableAsset>(programOwner).GetUsersGTAssetId(assetOwner, 1);

            if(actualAssetId2 != assetId2) {
                Error.Throw("Wrong asset written to user asset list!");
            }

            // Set auction address to this contract address
            ProgramHelper.Program<TradableAsset>(programOwner).SetAuction(Info.ProgramAddress());

            // Transfer the item and check if storage is alright
            Bytes newAssetOwner = new Bytes("0000000000000000000000000000000000000000000000000000000000000000");
            ProgramHelper.Program<TradableAsset>(programOwner).TransferGTAsset(assetId1, newAssetOwner);

            // Check if asset was actually taken
            assetCounter = ProgramHelper.Program<TradableAsset>(programOwner).GetGTUsersAssetCount(assetOwner);
            if(assetCounter != 1) {
                Error.Throw("Transfer went wrong! (wrong assets amount)");
            }

            // Check if last asset was moved to the first slot
            long movedAssetId = ProgramHelper.Program<TradableAsset>(programOwner).GetUsersGTAssetId(assetOwner, 0);
            if(movedAssetId != assetId2) {
                Error.Throw("Transfer went wrong! (wrong asset on slot 1)");
            }
        }
    }
}
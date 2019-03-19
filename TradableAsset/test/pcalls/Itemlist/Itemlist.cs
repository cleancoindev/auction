namespace Expload.Standards {

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
            Bytes classId = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            Bytes instanceId = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            long assetId1 = ProgramHelper.Program<TradableXGAsset>(programOwner).EmitXGAsset(
                assetOwner, classId, instanceId
            );

            // Emit one more asset
            long assetId2 = ProgramHelper.Program<TradableXGAsset>(programOwner).EmitXGAsset(
                assetOwner, classId, instanceId
            );

            // Get asset counter
            long assetCounter = ProgramHelper.Program<TradableXGAsset>(programOwner).GetUsersXGAssetCount(assetOwner);

            if(assetCounter != 2) {
                Error.Throw("Wrong amount of assets emitted!");
            }

            // Get id of first emitted asset
            long actualAssetId1 = ProgramHelper.Program<TradableXGAsset>(programOwner).GetUsersXGAssetId(assetOwner, 0);

            if(actualAssetId1 != assetId1) {
                Error.Throw("Wrong asset written to user asset list!");
            }

            // Get id of second emitted asset
            long actualAssetId2 = ProgramHelper.Program<TradableXGAsset>(programOwner).GetUsersXGAssetId(assetOwner, 1);

            if(actualAssetId2 != assetId2) {
                Error.Throw("Wrong asset written to user asset list!");
            }

            // Set auction address to this contract address
            ProgramHelper.Program<TradableXGAsset>(programOwner).SetAuction(Info.ProgramAddress());

            // Transfer the item and check if storage is alright
            Bytes newAssetOwner = new Bytes("0000000000000000000000000000000000000000000000000000000000000000");
            ProgramHelper.Program<TradableXGAsset>(programOwner).TransferXGAsset(assetId1, newAssetOwner);

            // Check if asset was actually taken
            assetCounter = ProgramHelper.Program<TradableXGAsset>(programOwner).GetUsersXGAssetCount(assetOwner);
            if(assetCounter != 1) {
                Error.Throw("Transfer went wrong! (wrong assets amount)");
            }

            // Check if last asset was moved to the first slot
            long movedAssetId = ProgramHelper.Program<TradableXGAsset>(programOwner).GetUsersXGAssetId(assetOwner, 0);
            if(movedAssetId != assetId2) {
                Error.Throw("Transfer went wrong! (wrong asset on slot 1)");
            }
        }
    }
}
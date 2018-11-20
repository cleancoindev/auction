using Expload.Pravda;
using System;

namespace PcallNamespace {

    [Program]
    public class itemlist {
        public static int Main() {return 1;}

        public void test_itemlist() {
            // Init addresses and get program by address
            Bytes programOwner = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes assetOwner   = new Bytes("8743e40cd8a5e162272fc8a5c56595b3aa9abb9708f26abc88f1d61cbb5576dc");

            // Emit the asset
            Bytes externalId = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            Bytes metaId = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            UInt32 assetId_1 = ProgramHelper.Program<PASS>(programOwner).EmitGTAsset(
                assetOwner, externalId, metaId
            );

            // Emit one more asset
            UInt32 assetId_2 = ProgramHelper.Program<PASS>(programOwner).EmitGTAsset(
                assetOwner, externalId, metaId
            );

            // Get asset counter
            UInt32 assetCounter = ProgramHelper.Program<PASS>(programOwner).getGTUsersAssetCount(assetOwner);

            if(assetCounter != 2) {
                Error.Throw("Wrong amount of assets emitted!");
            }

            // Get id of first emitted asset
            UInt32 actualAssetId_1 = ProgramHelper.Program<PASS>(programOwner).getUsersGTAssetId(assetOwner, 0);

            if(actualAssetId_1 != assetId_1) {
                Error.Throw("Wrong asset written to user asset list!");
            }

            // Get id of second emitted asset
            UInt32 actualAssetId_2 = ProgramHelper.Program<PASS>(programOwner).getUsersGTAssetId(assetOwner, 1);

            if(actualAssetId_2 != assetId_2) {
                Error.Throw("Wrong asset written to user asset list!");
            }

            // Set auction address to this contract address
            ProgramHelper.Program<PASS>(programOwner).SetAuction(Info.ProgramAddress());

            // Transfer the item and check if storage is alright
            Bytes newAssetOwner = new Bytes("0000000000000000000000000000000000000000000000000000000000000000");
            ProgramHelper.Program<PASS>(programOwner).TransferGTAsset(assetId_1, newAssetOwner);

            // Check if asset was actually taken
            assetCounter = ProgramHelper.Program<PASS>(programOwner).getGTUsersAssetCount(assetOwner);
            if(assetCounter != 1) {
                Error.Throw("Transfer went wrong! (wrong assets amount)");
            }

            // Check if last asset was moved to the first slot
            UInt32 movedAssetId = ProgramHelper.Program<PASS>(programOwner).getUsersGTAssetId(assetOwner, 0);
            if(movedAssetId != assetId_2) {
                Error.Throw("Transfer went wrong! (wrong asset on slot 1)");
            }
        }
    }
}
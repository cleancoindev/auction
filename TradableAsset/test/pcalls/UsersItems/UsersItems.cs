namespace Expload.Standarts {

    using Pravda;
    using System;

    [Program]
    public class UsersItems {
        public static void Main() {}

        public string test_UsersItems(){
            // Init addresses and get program by address
            Bytes programOwner = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes assetOwner = new Bytes("a1fe824f193bcee32f33b9e01245bd41f05a157eca73daf65d70ebd27430836d");

            // Set auction address to this contract address
            ProgramHelper.Program<TradableAsset>(programOwner).SetAuction(Info.ProgramAddress());

            // Emit 3 assets
            Bytes externalId = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            Bytes metaId = new Bytes("0000000000000000000000000000000000000000000000000000000000000001");
            UInt32 assetId = ProgramHelper.Program<TradableAsset>(programOwner).EmitGTAsset(
                assetOwner, externalId, metaId
            );

            externalId = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            metaId = new Bytes("0000000000000000000000000000000000000000000000000000000000000002");
            assetId = ProgramHelper.Program<TradableAsset>(programOwner).EmitGTAsset(
                assetOwner, externalId, metaId
            );

            externalId = new Bytes("0000000000000000000000000000000000000000000000000000000000000003");
            metaId = new Bytes("0000000000000000000000000000000000000000000000000000000000000003");
            assetId = ProgramHelper.Program<TradableAsset>(programOwner).EmitGTAsset(
                assetOwner, externalId, metaId
            );

            // Return JSON data
            return ProgramHelper.Program<TradableAsset>(programOwner).GetUsersAllGTAssetsData(assetOwner);
        }
    }
}
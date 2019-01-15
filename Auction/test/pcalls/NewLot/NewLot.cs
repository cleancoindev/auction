namespace Expload {

    using Pravda;
    using Standards;
    using System;

    [Program]
    public class NewLot {
        public static void Main() { }

        public Lot[] test_NewLot() {
            // Init addresses and get program by address
            var tradableAssetAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            var tradableGTAssetAdress = new Bytes("17e22f66979eca19a8b060a8bb759bfb3dbbce785a039e9e1ed01a54cc92161c");
            var auctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            // Make 3 lots
            var lotId = ProgramHelper.Program<Auction>(auctionAddress).CreateLot(1, false, 1, 200);
            var lotId2 = ProgramHelper.Program<Auction>(auctionAddress).CreateLot(1, false, 2, 200);
            var lotId3 = ProgramHelper.Program<Auction>(auctionAddress).CreateLot(1, true, 1, 200);

            // Check if asset was transfered to auction wallet
            if(auctionAddress != ProgramHelper.Program<TradableXCAsset>(tradableAssetAddress).GetXCAssetOwner(1)){
                Error.Throw("Asset was not transfered to auction wallet.");
            }
            if(auctionAddress != ProgramHelper.Program<TradableGTAsset>(tradableGTAssetAdress).GetGTAssetOwner(1)){
                Error.Throw("Asset was not transfered to auction wallet.");
            }

            // Check if lot was put to user storage
            if(lotId != ProgramHelper.Program<Auction>(auctionAddress).GetUserLotId(Info.Sender(), 0)){
                Error.Throw("Asset was not put to user storage");
            }

            // Check if lot was put to asset storage
            if(lotId != ProgramHelper.Program<Auction>(auctionAddress).GetAssetLotId(
                1, new Bytes("0000000000000000000000000000000000000000000000000000000000000001"), 0)
            ){
                Error.Throw("Asset was not put to asset storage");
            }

            // Check lot object
            return ProgramHelper.Program<Auction>(auctionAddress).GetUserLotsData(Info.Sender());
        }
    }
}
namespace Expload {

    using Pravda;
    using Standarts;
    using System;

    [Program]
    public class NewLot {
        public static void Main() { }

        public string test_NewLot() {
            // Init addresses and get program by address
            var tradableAssetAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            var auctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            // Make 2 lots
            var lotId = ProgramHelper.Program<Auction>(auctionAddress).CreateLot(1, 1, 200);
            var lotId2 = ProgramHelper.Program<Auction>(auctionAddress).CreateLot(1, 2, 200);

            // Check if asset was transfered to auction wallet
            if(auctionAddress != ProgramHelper.Program<TradableAsset>(tradableAssetAddress).GetXCAssetOwner(1)){
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

            // Check lot json dump
            return ProgramHelper.Program<Auction>(auctionAddress).GetUserLotsData(Info.Sender());
        }
    }
}
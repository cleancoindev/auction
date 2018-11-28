namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class newlot {
        public static void Main() { }

        public string test_newlot() {
            // Init addresses and get program by address
            Bytes PassAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes AuctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            // Make 2 lots
            UInt32 lotId = ProgramHelper.Program<Auction>(AuctionAddress).createLot(1, 1, 200);
            UInt32 lotId_2 = ProgramHelper.Program<Auction>(AuctionAddress).createLot(1, 2, 200);

            // Check if asset was transfered to auction wallet
            if(AuctionAddress != ProgramHelper.Program<PASS>(PassAddress).getXCAssetOwner(1)){
                Error.Throw("Asset was not transfered to auction wallet.");
            }

            // Check if lot was put to user storage
            if(lotId != ProgramHelper.Program<Auction>(AuctionAddress).getUserLotId(Info.Sender(), 0)){
                Error.Throw("Asset was not put to user storage");
            }

            // Check if lot was put to asset storage
            if(lotId != ProgramHelper.Program<Auction>(AuctionAddress).getAssetLotId(
                1, new Bytes("0000000000000000000000000000000000000000000000000000000000000001"), 0)
            ){
                Error.Throw("Asset was not put to asset storage");
            }

            // Check lot json dump
            return ProgramHelper.Program<Auction>(AuctionAddress).getUserLotsDataData(Info.Sender());
        }
    }
}
namespace Expload {

    using Pravda;
    using Standarts;
    using System;

    [Program]
    public class Buy {
        public static void Main() { }

        public string test_Buy() {
            // Init addresses and get program by address
            Bytes auctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            // Old auction balance
            long auctionBalance = Info.Balance(auctionAddress);

            // Buy a lot
            ProgramHelper.Program<Auction>(auctionAddress).BuyLot(1);

            // Check if the money was transfered
            if(auctionBalance + 200 != Info.Balance(auctionAddress)){
                Error.Throw("Coins were not transfered correctly");
            }
            
            Error.Throw("breakpoint");

            // Check if the bid status was updated
            return ProgramHelper.Program<Auction>(auctionAddress).GetLotData(1);
        }
    }
}
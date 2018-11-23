namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class bid {
        public static void Main() { }

        public string test_bid() {
            // Init addresses and get program by address
            Bytes PassAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes AuctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            // Old auction balance
            long auctionBalance = Info.Balance(AuctionAddress);

            // Make 2 bids
            ProgramHelper.Program<Auction>(AuctionAddress).makeBid(1, 200);
            ProgramHelper.Program<Auction>(AuctionAddress).makeBid(2, 200);

            // Check if the money was transfered
            if(auctionBalance + 400 != Info.Balance(AuctionAddress)){
                Error.Throw("Coins were not transfered correctly");
            }

            // Check if the bid status was updated
            return ProgramHelper.Program<Auction>(AuctionAddress).getLotData(1);
        }
    }
}
namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class buy {
        public static void Main() { }

        public string test_buy() {
            // Init addresses and get program by address
            Bytes PassAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes AuctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            // Old auction balance
            long auctionBalance = Info.Balance(AuctionAddress);

            // Buy a lot
            ProgramHelper.Program<Auction>(AuctionAddress).buyLot(1);

            // Check if the money was transfered
            if(auctionBalance + 200 != Info.Balance(AuctionAddress)){
                Error.Throw("Coins were not transfered correctly");
            }

            // Check if the bid status was updated
            return ProgramHelper.Program<Auction>(AuctionAddress).getLotData(1);
        }
    }
}
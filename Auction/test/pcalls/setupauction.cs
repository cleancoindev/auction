namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class setupauction {
        public static void Main() { }

        public void test_setupauction() {
            // Init addresses and get program by address
            Bytes PassAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes AuctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            ProgramHelper.Program<Auction>(AuctionAddress).addGame(PassAddress);
        }
    }
}
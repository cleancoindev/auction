namespace Expload {

    using Pravda;
    using Standarts;
    using System;

    [Program]
    public class SetUpAuction {
        public static void Main() { }

        public void test_SetUpAuction() {
            // Init addresses and get program by address
            var tradableAssetAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            var auctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            ProgramHelper.Program<Auction>(auctionAddress).AddGame(tradableAssetAddress);
        }
    }
}
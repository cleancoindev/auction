namespace Expload {

    using Pravda;
    using Standards;
    using System;

    [Program]
    public class SetUpAuction {
        public static void Main() { }

        public void test_SetUpAuction() {
            // Init addresses and get program by address
            var tradableXGAssetAddress = new Bytes("17e22f66979eca19a8b060a8bb759bfb3dbbce785a039e9e1ed01a54cc92161c");
            var tradableAssetAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            var auctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");

            ProgramHelper.Program<Auction>(auctionAddress).AddGame(tradableXGAssetAddress, true);
            ProgramHelper.Program<Auction>(auctionAddress).AddGame(tradableAssetAddress, false);
            
            // Set up XG address
            var XGAddress = new Bytes("64a818e62d78f7b2642b0535db69c9b7e7aff0f12562110bdeeea082dc217f29");
            ProgramHelper.Program<Auction>(auctionAddress).SetXGAddress(XGAddress);
        }
    }
}
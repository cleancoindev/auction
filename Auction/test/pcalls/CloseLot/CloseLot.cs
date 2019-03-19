namespace Expload {

    using Pravda;
    using Standards;
    using System;

    [Program]
    public class CloseLot {
        public static void Main() { }

        public void test_CloseLot() {
            // Init addresses and get program by address
            var tradableAssetAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            var auctionAddress = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");
            var userAddress = new Bytes("8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f");

            // Close the lot
            ProgramHelper.Program<Auction>(auctionAddress).CloseLot(2);

            // Check if the asset went back
            if(userAddress != ProgramHelper.Program<TradableXPAsset>(tradableAssetAddress).GetXPAssetOwner(2)){
                Error.Throw("Asset was not returned.");
            }
        }
    }
}
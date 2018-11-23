namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class closelot {
        public static void Main() { }

        public void test_closelot() {
            // Init addresses and get program by address
            Bytes PassAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes AuctionAddress = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");
            Bytes userAddress = new Bytes("8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f");
            Bytes bidderAddress = new Bytes("edbfca5b9a253738634352c465b2f0ea1a2f280dbf5510bd83010798dd203996");

            // Get bidder's old balance
            long biddersBalance = Info.Balance(bidderAddress);

            // Close the lot
            ProgramHelper.Program<Auction>(AuctionAddress).closeLot(2);

            // Check if the money went back
            if(biddersBalance + 200 != Info.Balance(bidderAddress)){
                Error.Throw("Money was not returned.");
            }

            // Check if the asset went back
            if(userAddress != ProgramHelper.Program<PASS>(PassAddress).getXCAssetOwner(2)){
                Error.Throw("Asset was not returned.");
            }
        }
    }
}
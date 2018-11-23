namespace auction {

    using Expload.Pravda;
    using System;

    [Program]
    public class timeout {
        public static void Main() { }

        public void test_timeout() {
            // Init addresses and get program by address
            Bytes PassAddress = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            Bytes AuctionAddress = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");
            Bytes userAddress = new Bytes("8fc47de7507f0881fb0133cbbd82733b69426b1b55904907f3de3dbfb262210f");
            Bytes bidderAddress = new Bytes("edbfca5b9a253738634352c465b2f0ea1a2f280dbf5510bd83010798dd203996");

            // Get lot creator's old balance
            long creatorsBalance = Info.Balance(userAddress);

            // Close the lot
            ProgramHelper.Program<Auction>(AuctionAddress).timeoutLot(1);

            // Check if the money was transfered
            if(creatorsBalance + 200 != Info.Balance(userAddress)){
                Error.Throw("Money was not transfered.");
            }

            // Check if the asset was transfered
            if(bidderAddress != ProgramHelper.Program<PASS>(PassAddress).getXCAssetOwner(1)){
                Error.Throw("Asset was not transfered.");
            }
        }
    }
}
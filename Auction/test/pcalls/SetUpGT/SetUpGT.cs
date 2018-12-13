namespace Expload {

    using Pravda;
    using Standarts;
    using System;

    [Program]
    public class SetUpGT {
        public static void Main() { }

        public void test_SetUpGT() {
            // Init addresses and get program by address
            var auctionAddress = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");
            var GTAddress = new Bytes("64a818e62d78f7b2642b0535db69c9b7e7aff0f12562110bdeeea082dc217f29");
            var buyerAddress = new Bytes("edbfca5b9a253738634352c465b2f0ea1a2f280dbf5510bd83010798dd203996");
            
            // Add program owner and auction to white list
            ProgramHelper.Program<GameToken>(GTAddress).WhiteListAdd(GTAddress);
            ProgramHelper.Program<GameToken>(GTAddress).WhiteListAdd(auctionAddress);
            
            // Give some GT to buyer
            ProgramHelper.Program<GameToken>(GTAddress).Give(buyerAddress, 10000);
        }
    }
}
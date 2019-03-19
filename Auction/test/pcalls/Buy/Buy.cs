namespace Expload {

    using Pravda;
    using Standards;
    using System;

    [Program]
    public class Buy {
        public static void Main() { }

        public Lot test_Buy() {
            // Init addresses and get program by address
            var auctionAddress   = new Bytes("e04919086e3fee6f1d8f6247a2c0b38f874ab40a50ad2c62775fb09baa05e342");
            var XGAddress = new Bytes("64a818e62d78f7b2642b0535db69c9b7e7aff0f12562110bdeeea082dc217f29");

            // Buy a lot
            ProgramHelper.Program<Auction>(auctionAddress).BuyLot(1);
            
            // Old user balance
            var userBalance = ProgramHelper.Program<XGold>(XGAddress).MyBalance();
            
            // Buy a XG lot
            ProgramHelper.Program<Auction>(auctionAddress).BuyLot(3);
            
            // Check if XG were spent
            if (userBalance - 200 != ProgramHelper.Program<XGold>(XGAddress).MyBalance())
            {
                Error.Throw("XGs were not transfered correctly");
            }

            // Check if the bid status was updated
            return ProgramHelper.Program<Auction>(auctionAddress).GetLotData(1);
        }
    }
}
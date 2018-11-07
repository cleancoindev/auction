using Expload.Pravda;
using System;

namespace PcallNamespace {

    [Program]
    public class transfer {
        public static void Main() {}

        public bool test_transfer(){
            Bytes address = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            UInt32 id = ProgramHelper.Program<PASS>(address).EmitAsset(
                1, true, "TestItemName", "TestItemDesc", Info.Sender()
            );
            Bytes newOwner = new Bytes("4204abb6a8b7ca81b7595b0f3e67329616d2a1ccf6d5e6bbcd0d45733df0f234");
            ProgramHelper.Program<PASS>(address).TransferAsset(id, newOwner);
            Bytes itemOwner = ProgramHelper.Program<PASS>(address).getOwner(id);
            return (newOwner == itemOwner);
        }
    }
}
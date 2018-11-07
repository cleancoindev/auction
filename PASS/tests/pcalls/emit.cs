using Expload.Pravda;
using System;

namespace PcallNamespace {

    [Program]
    public class emit {
        public static int Main() {return 1;}

        public bool test_emit() {
            Bytes address = new Bytes("fb75559bb4bb172ca0795e50b390109a50ce794466a14c24c73acdb40604065b");
            UInt32 id = ProgramHelper.Program<PASS>(address).EmitAsset(
                1, true, "TestItemName", "TestItemDesc", Info.Sender()
            );
            Bytes itemOwner = ProgramHelper.Program<PASS>(address).getOwner(id);
            return (Info.Sender() == itemOwner);
        }
    }
}
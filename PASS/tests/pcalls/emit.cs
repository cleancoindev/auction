using Expload.Pravda;
using System;

namespace PcallNamespace {

    [Program]
    public class Emit {
        public static void Main() {}

        public bool TestEmit() {
            Bytes address = new Bytes("<>");
            UInt32 id = ProgramHelper.Program<PASS>(address).EmitAsset(
                1, true, "TestItemName", "TestItemDesc", Info.Sender()
            );
            Bytes itemOwner = ProgramHelper.Program<PASS>(address).getOwner(id);
            return (Info.Sender() == itemOwner);
        }
    }
}
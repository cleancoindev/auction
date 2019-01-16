namespace Expload.Standards
{
    using Pravda;
    
    public class Asset {
        /*
        Class defining a game asset
        */
        
        public Asset(long id, Bytes owner, Bytes classId, Bytes instanceId){
            this.Id = id;
            this.Owner = owner;
            this.ItemClassId = classId;
            this.ItemInstanceId = instanceId;
        }
        
        public Asset() { }

        // Asset's blockchain id
        public long Id { get; set; } = 0;

        // Address of asset's owner
        public Bytes Owner { get; set; } = Bytes.VOID_ADDRESS;

        // Game's internal asset class id
        // E.g. two swords of same type but
        // with different upgrades have same
        // ItemClassId
        public Bytes ItemClassId { get; set; } = Bytes.VOID_ADDRESS;

        // Game's internal asset instance id
        // E.g. two swords of same type but
        // with different upgrades have different
        // ItemInstanceId
        public Bytes ItemInstanceId { get; set; } = Bytes.VOID_ADDRESS;
    }
}
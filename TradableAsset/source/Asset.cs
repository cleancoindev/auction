namespace Expload.Standards
{
    using Pravda;
    
    public class Asset {
        /*
        Class defining a game asset
        */
        
        public Asset(long id, Bytes owner, Bytes externalId, Bytes metaId){
            this.Id = id;
            this.Owner = owner;
            this.ExternalId = externalId;
            this.MetaId = metaId;
        }
        
        public Asset() { }

        // Asset's blockchain id
        public long Id { get; set; } = 0;

        // Address of asset's owner
        public Bytes Owner { get; set; } = Bytes.VOID_ADDRESS;

        // Game's external asset id
        // E.g. two identical in-game swords
        // Have same internal game id
        public Bytes ExternalId { get; set; } = Bytes.VOID_ADDRESS;

        // External meta-data identifier
        public Bytes MetaId { get; set; } = Bytes.VOID_ADDRESS;
    }
}
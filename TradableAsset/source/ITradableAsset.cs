using Expload.Pravda;

namespace Expload.Standarts
{
    public interface ITradableAsset
    {
        /// <summary>
        /// Set up Expload Auction address
        /// </summary>
        /// <param name="address"> Auction address </param>
        void SetAuction(Bytes address);
    }
}
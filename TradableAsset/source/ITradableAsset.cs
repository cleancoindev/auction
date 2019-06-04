using Expload.Pravda;

namespace Expload.Standards
{
    public interface ITradableAsset
    {
        /// <summary>
        /// Get asset class meta data using his classId
        /// </summary>
        /// <param name="classId"> Asset class id </param>
        string GetClassIdMeta(Bytes classId);

        /// <summary>
        /// Get asset instance meta data using his instanceId
        /// </summary>
        /// <param name="instanceId"> Asset instance id </param>
        string GetInstanceIdMeta(Bytes instanceId);

        /// <summary>
        /// Set up Expload Auction address
        /// </summary>
        /// <param name="address"> Auction address </param>
        void SetAuction(Bytes address);

        /// <summary>
        /// Set up commission for assets
        /// </summary>
        /// <param name="percent"> Percent of commission </param>
        void SetCommission(long percent);

        /// <summary>
        /// Get percent of commission
        /// </summary>
        /// <returns> Percent of commission </returns>
        long GetCommission();
    }
}
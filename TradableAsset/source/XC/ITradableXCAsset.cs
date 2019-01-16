using Expload.Pravda;

namespace Expload.Standards
{
    public interface ITradableXCAsset : ITradableAsset
    {
        /// <summary>
        /// Get XC asset data
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Asset object
        /// </returns>
        Asset GetXCAssetData(long id);

        /// <summary>
        /// Get XC asset owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Owner address
        /// </returns>
        Bytes GetXCAssetOwner(long id);

        /// <summary>
        /// Get XC asset class id
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Class id
        /// </returns>
        Bytes GetXCAssetClassId(long id);

        /// <summary>
        /// Get amount of XC assets belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// Asset amount
        /// </returns>
        long GetUsersXCAssetCount(Bytes address);

        /// <summary>
        /// Get asset id of a particular XC asset belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <param name="number"> Asset serial number </param>
        /// <returns>
        /// Asset id
        /// </returns>
        long GetUsersXCAssetId(Bytes address, long number);

        /// <summary>
        /// Get list of XC assets
        /// belonging to a particular user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// List of asset objects
        /// </returns>
        Asset[] GetUsersAllXCAssetsData(Bytes address);

        /// <summary>
        /// Emit a XC asset
        /// </summary>
        /// <param name="owner"> Desired asset owner </param>
        /// <param name="classId"> Asset class id </param>
        /// <param name="instanceId"> Asset instance id </param>
        /// <returns>
        /// Emitted asset id
        /// </returns>
        long EmitXCAsset(Bytes owner, Bytes classId, Bytes instanceId);

        /// <summary>
        /// Transfer XC asset to a new owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <param name="to"> New owner address </param>
        void TransferXCAsset(long id, Bytes to);
    }
}
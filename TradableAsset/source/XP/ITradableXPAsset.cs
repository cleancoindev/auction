using Expload.Pravda;

namespace Expload.Standards
{
    public interface ITradableXPAsset : ITradableAsset
    {
        /// <summary>
        /// Get XP asset data
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Asset object
        /// </returns>
        Asset GetXPAssetData(long id);

        /// <summary>
        /// Get XP asset owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Owner address
        /// </returns>
        Bytes GetXPAssetOwner(long id);

        /// <summary>
        /// Get XP asset class id
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Class id
        /// </returns>
        Bytes GetXPAssetClassId(long id);

        /// <summary>
        /// Get amount of XP assets belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// Asset amount
        /// </returns>
        long GetUsersXPAssetCount(Bytes address);

        /// <summary>
        /// Get asset id of a particular XP asset belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <param name="number"> Asset serial number </param>
        /// <returns>
        /// Asset id
        /// </returns>
        long GetUsersXPAssetId(Bytes address, long number);

        /// <summary>
        /// Get list of XP assets
        /// belonging to a particular user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// List of asset objects
        /// </returns>
        Asset[] GetUsersAllXPAssetsData(Bytes address);

        /// <summary>
        /// Emit a XP asset
        /// </summary>
        /// <param name="owner"> Desired asset owner </param>
        /// <param name="classId"> Asset class id </param>
        /// <param name="instanceId"> Asset instance id </param>
        /// <returns>
        /// Emitted asset id
        /// </returns>
        long EmitXPAsset(Bytes owner, Bytes classId, Bytes instanceId);

        /// <summary>
        /// Transfer XP asset to a new owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <param name="to"> New owner address </param>
        void TransferXPAsset(long id, Bytes to);
    }
}
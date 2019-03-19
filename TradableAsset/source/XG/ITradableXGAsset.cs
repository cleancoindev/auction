using Expload.Pravda;

namespace Expload.Standards
{
    public interface ITradableXGAsset : ITradableAsset
    {
        /// <summary>
        /// Get XG asset data
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Asset object
        /// </returns>
        Asset GetXGAssetData(long id);

        /// <summary>
        /// Get XG asset owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Owner address
        /// </returns>
        Bytes GetXGAssetOwner(long id);

        /// <summary>
        /// Get XG asset class id
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <returns>
        /// Class id
        /// </returns>
        Bytes GetXGAssetClassId(long id);

        /// <summary>
        /// Get amount of XG assets belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// Asset amount
        /// </returns>
        long GetUsersXGAssetCount(Bytes address);

        /// <summary>
        /// Get asset id of a particular XG asset belonging to a user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <param name="number"> Asset serial number </param>
        /// <returns>
        /// Asset id
        /// </returns>
        long GetUsersXGAssetId(Bytes address, long number);

        /// <summary>
        /// Get list of XG assets
        /// belonging to a particular user
        /// </summary>
        /// <param name="address"> User address </param>
        /// <returns>
        /// List of asset objects
        /// </returns>
        Asset[] GetUsersAllXGAssetsData(Bytes address);

        /// <summary>
        /// Emit a XG asset
        /// </summary>
        /// <param name="owner"> Desired asset owner </param>
        /// <param name="classId"> Asset class id </param>
        /// <param name="instanceId"> Asset instance id </param>
        /// <returns>
        /// Emitted asset id
        /// </returns>
        long EmitXGAsset(Bytes owner, Bytes classId, Bytes instanceId);

        /// <summary>
        /// Transfer XG asset to a new owner
        /// </summary>
        /// <param name="id"> Asset id </param>
        /// <param name="to"> New owner address </param>
        void TransferXGAsset(long id, Bytes to);
    }
}
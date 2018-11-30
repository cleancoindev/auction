# PASS - Pravda Asset Storage Standart
## Overview
PASS is a token standart for storing and operating with assets on Pravda platform.  
The standart is a simplified and adjusted to suit Expload ecosystem version of ERC-721 non-fungible token standart.  
Currently, interfaces are not supported in Pravda, so the standart is provided by a sample implementation of it. The implementation can suit almost any blockchain gaming experience, and minor changes are required for integrating PASS.
## Program Structure
### Global variables and setup
There are several things you are to change before deploying PASS and using it with your game:
- Set `auctionAddress` variable if you want your game to work with Expload Auction. It can be edited any time using `SetAuction` method, so leave it as it is if you're not sure about your plans on auction yet.
- Alter `getMetaData` function for it to return valid meta data URL when given in-game item's meta id. Meta data has following format:
    ```json
    {
        "name": <itemName>,
        "desc": <itemShortDescription>,
        "pic": <itemPictureURL>,
        "misc": <miscData>
    }
    ```
    `itemPictureURL` should link to a valid item icon, and `miscData` includes full item description, its in-game stats, etc.  
    When `metaId` of an asset is passed to program's `getMetaData` function, a URL linking to asset's meta data should be returned. This meta is to be shown in Expload Desktop app.

### Asset class
Represents an actual non-fungible token. Attributes:
- `UInt32 id` - blockchain id of the asset
- `Bytes owner` - address of asset owner
- `Bytes externalId` - game's internal asset id. For example, two completely identical in-game swords have a same external id.
- `Bytes metaId` - asset's meta data id. Used to get asset's meta data using `getMetaData` method.

### Asset storage
All of the storage mappings come in 2 ways: for storing GT assets (assets which can only be sold and bought with GameToken) and storing XC assets (assests which can only be sold and bought with XCoin).  
The storage is also split into two logical parts:
- Main storage, where asset data is stored. Each asset has its unqiue blockchain ID, which is given when an asset is emitted. For example, two completely identical in-game swords have different blockchain IDs. `BlockchainID = 0` is invalid. 
    - You can get any asset data by its blockchain ID by calling `getGTAssetData` \ `getXCAssetData` method, which returns JSON.
    - You can also get asset `owner` or `externalId` by calling `getGTAssetOwner` \ `getXCAssetOwner` or `getGTAssetExternalId` \ `getXCAssetExternalId` method. 
    - Amount of assets emitted is stored in `lastGTId` and `lastXCId` variables.
- User asset storage, which allows you to get the assets belonging to a particular user. 
    - You can get the amount of assets owned by a user by calling `getGTUsersAssetCount` \ `getXCUsersAssetCount`. 
    - You can get blockchain ID of a particular asset belonging to a user by calling `getUsersGTAssetId` \ `getUsersXCAssetId`. 
    - You can get a JSON with all of user's assets by calling `getUsersAllGTAssetsData` \ `getUsersAllXCAssetsData`.

### Interacting with the storage
As well as the storage, all interaction methods are split into two sections: GT and XC.
- `EmitGTAsset` \ `EmitXCAsset` - emit an asset. Can only be called by PASS contract owner. Logs `EmitGT` \ `EmitXC` event.
- `TransferGTAsset` \ `TransferXCAsset` - transfer an asset to a different wallet. Can only be called from auction contract, e.g. if a user puts his asset up for sale, the asset is transfered to auction's wallet. Emits `TransferGT` \ `TransferXC` event.

### FAQ
- *I've deployed and set up PASS contract for my game. How do I call it from user's game client?*  
You can use [DApp API](https://expload.com/developers/documentation/pravda/integration/dapp-api/) to provide user interaction with the contract.
- *I've set up auction address correctly, but Expload Auction doesn't seem to work with the game correctly. Why?*
Expload Auction is only availible to certified providers, please contact us at dev@expload.com for more info. 

If you still have questions, make sure to read comments and docstring in PASS contract sample implementation.
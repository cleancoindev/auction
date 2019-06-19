# TradableAsset standard

## Overview

TradableAsset is a token standard for storing and operating with assets on Pravda platform.  
Asset is a non-fungible token. The implementation can suit almost any blockchain gaming experience, and minor changes are required for integrating TradableAsset.  
There are 2 types of assets - XGold Assets (or XG Assets for short), which can only be bought and sold for XGold, and XPlatinum Assets (or XP Assets for short) which can only be sold for XPlatinum.
The standard is provided by [ITradableAsset](https://github.com/expload/auction/blob/master/TradableAsset/source/ITradableAsset.cs) interface. Sample implementation is also available for both [XGold Asset](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/TradableXGAsset.cs) and [XPlatinum Asset](https://github.com/expload/auction/blob/master/TradableAsset/source/XP/TradableXPAsset.cs).

## Program Structure

There we're going to cover sample TradableAsset implementations.
Your implementation may have a different structure and functioning, though using approaches and patterns from the sample is highly recommended.

### Global variables and setup

There are several things you are to change before deploying TradableAsset and using it with your game:

- Set `auctionAddress` variable if you want your game to work with Expload Auction. It can be edited any time using `SetAuction` method, so leave it as it is if you're not sure about your plans on auction yet.

- Alter `GetClassIdMeta` and `GetInstanceIdMeta` functions for them to return valid meta data URLs when given in-game item's class or instance id. This meta is to be shown in Expload Desktop app. Meta data has following format:

    ```json
    {
        "name": <itemName>,
        "desc": <itemShortDescription>,
        "pic":  <itemPictureURL>,
        "misc": <miscData>
    }
    ```

    `itemPictureURL` should link to a valid item icon, and `miscData` includes full item description, its in-game stats, etc.

    All item's stats and game-specific attributes should be put in `misc` section of meta.

    Instance id meta may not include data from class id meta. If included, it will override class id meta.

### Asset class

Represents an actual non-fungible token. Attributes:

- `UInt32 Id` - blockchain id of the asset

- `Bytes Owner` - address of asset owner

- `Bytes ClassId` - game's internal asset class id. For example, two identical in-game swords, but with different upgrades, have a same class id.

- `Bytes InstanceId` - game's internal asset instance id. For example, two identical in-game swords, but with different upgrades, have different instance ids.

### Asset storage  

The storage is also split into two logical parts:

- Main storage, where asset data is stored. Each asset has its unique blockchain ID, which is given when an asset is emitted. For example, two completely identical in-game swords have different blockchain IDs. `BlockchainID = 0` is invalid.
  - You can get any asset data by its blockchain ID by calling `GetAssetData` method, which returns JSON.
  - You can also get asset `Owner` or `ClassId` by calling `GetAssetOwner` or `GetAssetClassId` method.
  - Amount of assets emitted is stored in `lastXGId` and `lastXPId` variables (depending on implementation).
- User asset storage, which allows you to get the assets belonging to a particular user.
  - You can get the amount of assets owned by a user by calling `GetUsersAssetCount`.
  - You can get blockchain ID of a particular asset belonging to a user by calling `GetUsersAssetId`.
  - You can get a list of all user's assets by calling `GetUsersAllAssetsData`.

### Interacting with the storage

- `EmitAsset` - emit an asset. Can only be called by TradableAsset contract owner. Logs `EmitXG` \ `EmitXP` event (depending on implementation).
- `TransferAsset` - transfer an asset to a different wallet. Can only be called from auction contract, e.g. if a user puts his asset up for sale, the asset is transfered to auction's wallet. Emits `TransferXG` or `TransferXP` event (depending on implementation).

### FAQ

- *I've deployed and set up TradableAsset contract for my game. How do I call it from user's game client?*

You can use [DApp API](https://expload.com/developers/documentation/pravda/integration/dapp-api/) to provide user interaction with the contract.

- *I've set up auction address correctly, but Expload Auction does not seem to work with the game correctly. Why?*

Expload Auction is only available to certified providers, please contact us at dev@expload.com for more info.
  
If you still have questions, make sure to read comments and docstring in TradableAsset contract sample implementation.

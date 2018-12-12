# Expload Auction
## Overview
Expload Auction is a universal market platform for all games on Expload platform. Expload Auction (together with TradableAsset) provides a united interface for in-game integration and interaction.

## Program structure
### Lot class
Represents a lot on the Auction. Attributes:
- `UInt32 Id` - blockchain id of the lot
- `Bytes Creator` - address lot creator
- `UInt32 GameId` - id of the game asset being sold is from
- `UInt32 AssetId` - blockchain id of the asset sold (see [TradableAsset docs](../TradableAsset/README.md))
- `Bytes ExternalId` - external id (game id) of the asset sold
- `UInt32 Price` - price of the lot
- `bool Closed` - if the lot was already sold or closed by its creator
- `Bytes Buyer` - null if asset hasn't been sold yet, buyer's adress otherwise

### Lot storage
The storage is split into three parts:
- Main lot data storage. Each lot has its unique lot id, which is given when a lot is created. `LotId = 0` is invalid.
    - You can get any lot data by its lot id by calling `GetLotData` method, which returns JSON.
- User lot storage, which allows you to get lots created by a particular user. 
    - You can get a JSONified list of user's lots by calling `GetUserLotsData` method.
- Asset lot storage, which allows you to get lots selling an asset with particular game id.
    - You can get a JSONified list of lots selling a particular in-game item by calling `GetAssetLotsData` method.

### Interacting with the storage
There are 3 ways to interact with the storage:
- Create a lot with `CreateLot` method. When this method is called, lot id is returned and `lotCreated` event is emitted. Also, the asset to be sold is transfered from lot creator to auction's wallet.
- Buy a lot with `BuyLot` method. When the method is called, asset is transfered to buyer and funds to lot creator, `lotBought` event is emitted.
- Close a lot with `CloseLot` method. When a lot is closed, the asset is returned to lot creator and the lot can't be bought anymore. Closed lot is not shown in Expload Auction UI and can't be reopened.

## Integrating your game
> Before working with Expload Auction, make sure your game is featuring an implementation of [TradableAsset](../TradableAsset/README.md)!

> Expload Auction is only availible to certified providers, so you are to contact dev@expload.com if your game doesn't have a unique `game id` for Expload Auction yet.

1. **Set up TradableAsset**
Use `SetAuction` method in TradableAsset to provide an up-to-date auction address.
2. **Integrate into UI**
    > To interact with Auction from your game's client, use [DApp API](https://expload.com/developers/documentation/pravda/integration/dapp-api/). Further description will only feature contract methods and will not focus on working with DApp API.

    Use `api/program/method-test` with methods which only recieve data from the auction and do not change the state of the contract. This will save watts and result in a better perfomance of the app. However, if you are to create, buy or close lots use `api/program/method` only, as otherwise changes will not be saved in blockchain.

If you still have questions, take a look at comments and docstrings in [Auction.cs](Auction.cs) or contact us at dev@expload.com.
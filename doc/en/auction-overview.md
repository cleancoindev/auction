# Working with Auction

Expload Auction is a common market platform for all the games on the Expload platform. Expload Auction (together with TradableAsset) provides a unified interface for in-game integration and interaction.

## Program Structure

### Lot Class

Represents a lot on the Auction with the following attributes:

- `UInt32 Id` — the blockchain id of the lot
- `Bytes Creator` — the address lot creator
- `UInt32 GameId` — the id of the game that the asset is being sold from
- `UInt32 AssetId` — the blockchain id of the asset sold (see [TradableAsset docs](../tradable-asset-standard/))
- `Bytes ClassId` — the class id of the asset sold
- `UInt32 Price` — the price of the lot
- `bool Closed` — the lot is already sold or closed by its creator
- `Bytes Buyer` — null if the asset hasn't been sold yet, the buyer's address otherwise stated

### Lot Storage

The storage is split into three parts:

- Main lot data storage. Each lot has its unique id, which is assigned when the lot is created. `LotId = 0` is invalid.
  - You can get any lot data by its id by calling the `GetLotData` method, which returns the lot object.
- User lot storage, which allows you to get lots created by a particular user.
- You can get a list of user's lots by calling the `GetUserLotsData` method.
- Asset lot storage, which allows you to get the lots selling an asset with a particular game id.
- You can get a list of lots selling a particular in-game item by calling `GetAssetLotsData` method.

### Interacting with the Storage

There are 3 ways to interact with the storage:

- Create a lot with the `CreateLot` method. When this method is called, the lot id is returned and the `lotCreated` event is generated. Also, the asset to be sold is transferred from the lot creator to the Auction's wallet.
- Buy a lot with the `BuyLot` method. When the method is called, the asset is transferred to the buyer and funds are transferred to the lot creator, the `lotBought` event is generated.
- Close the lot with the `CloseLot` method. When the lot is closed, the asset is returned to the lot creator and the lot can't be bought anymore. A closed lot is not shown in the Expload Auction UI and can't be reopened.

## Integrating your Game

> Before working with Expload Auction, make sure your game features an implementation of [TradableAsset](../tradable-asset-standard/)!
> Expload Auction is only available to certified providers, therefore please contact dev@expload.com if your game doesn't have a unique `game id` for Expload Auction yet.

1. **Set up TradableAsset**
  Use the `SetAuction` method in TradableAsset to provide an up-to-date auction address.
2. **Integrate into UI**
  > To interact with Auction from your game's client, use [DApp API](https://expload.com/developers/documentation/pravda/integration/dapp-api/). The description further will only feature contract methods and will not focus on working with DApp API.

Use `api/program/method-test` with methods which only receive data from the Auction and do not change the state of the contract. This will save watts and result in a better performance of the app. However, for the creation, purchasing or closing of lots only use `api/program/method`, otherwise the changes will not be saved in the blockchain.

If you still have questions, you can take a look at the comments and docstrings in [Auction.cs](https://github.com/expload/auction/blob/master/Auction/source/Auction.cs) or contact us at dev@expload.com.
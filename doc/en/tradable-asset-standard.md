# TradableAsset — Standard and Implementation Example

## Overview

For your game to be compatible with the Expload Auction and Inventory, all in-game items (or `assets`) should be stored in the Pravda program that inherits either [ITradableXGAsset interface](https://github.com/expload/Auction/blob/master/TradableAsset/source/XG/ITradableXGAsset.cs) or [ITradableXPAsset interface](https://github.com/expload/Auction/blob/master/TradableAsset/source/XP/ITradableXPAsset.cs).  
The interface `ITradableXGAsset` is suitable for `XGold Assets` — assets that can only be sold for XGold on the Expload Auction. Similarly, ITradableXPAsset is suitable for `XPlatinum Assets` that can only be sold for XPlatinum. The logic and structure of these interfaces are identical, the only difference being the naming (e.g. `GetXGAsset` and `GetXPAsset` have the same purpose).  
  
Let's have a closer look at these interfaces drawing on the [example of XGold Asset implementation](https://github.com/expload/Auction/blob/master/TradableAsset/source/XG/TradableXGAsset.cs).  
  
> Code snippets from the implementation are solely included for the deeper 
> understanding of the asset's inner logic, and reading through the snippets is not 
> necessary. If your game doesn't need any specific mechanics for the releasing or 
> or transferring of assets, you can just use the example implementation 
> as linked above in your game, including as an API. Check out the asset's 
> structure and method interface below and move on to [Deploy and Implementation Setup](#Deploy-and-Implementation-Setup).

## Asset Structure

Each asset has the following fields:

`long Id` (or `id`) — the blockchain-id of the asset, that doesn't have any in-game meaning and is only used inside Pravda programs as a unique asset identifier.

`Bytes Owner` — the address of the current asset owner.

`Bytes ItemClassId` (or `classId`) — the in-game id of a specific asset class. _For example, two in-game swords called "Deathbringer" may have different upgrades — one can be sharpened by a blacksmith and the other one can be enhanced by a wizard. This means that they have the same `classId` as they are the same class of an item, but their instances differ._ Assets with the equal `classId` will be put in a single Expload Auction tab.

`Bytes ItemInstanceId` (or `instanceId`) — the in-game id of a specific item instance. _For example, two in-game swords called "Deathbringer" may have different upgrades — one can be sharpened by a blacksmith and the other one can be enhanced by a wizard. This means that they have different `instanceId` as despite being the same item class, their instances differ._

## Asset Operations

### Asset Issuance

`EmitXGAsset(Bytes owner, Bytes classId, Bytes  instanceId) returns long id` — releases assets with specified parameters and gives them to the user with the address `Bytes owner`. Returns the unique asset `id` and can only be called by the Pravda program owner.

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method implementation (the comments are changed for the sake of convenience)

```c#
public long EmitXGAsset(Bytes owner, Bytes classId, Bytes instanceId){
    // Checking if the method caller
    // is the program owner
    AssertIsGameOwner();

    // _lastXGId - global variable storing 
    // the last id given to an asset
    // We increase it by 1
    var id = ++_lastXGId;

    // Create an asset object
    var asset = new Asset(id, owner, classId, instanceId);

    // Add the asset to _XGAssets mapping - main asset storage
    _XGAssets[id] = asset;

    /*
    In addition to the main asset storage, there is a special asset storage for each user, making it really easy to get all the assets owned by the user.
    */

    // Add the asset to the user’s storage.

    // Get the current amount of assets owned by the user
    // from _XGUsersAssetCount mapping
    var assetCount = _XGUsersAssetCount.GetOrDefault(owner, 0);

    // Store the asset id in the user storage
    _XGUsersAssetIds[GetUserAssetKey(owner, assetCount)] = id;
    // Increase user's asset count by 1
    _XGUsersAssetCount[owner] = assetCount + 1;
    // Add the asset serial number (its key in the user storage mapping) 
    // to the serial number storage
    // (so we don't have to iterate through all user's assets)
    _SerialNumbers[id] = assetCount;

    // Generate an event
    Log.Event("EmitXG", asset);

    // Return the asset's unique id
    return id;
}
```

</details>

### Asset Transference

`TransferXGAsset(long id, bytes to)` — transfers the asset with the specified `id` 
to address `to`. Can only be called by the Expload Auction.

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method implementation (the comments are changed for the sake of convenience)

```c#
public void TransferXGAsset(long id, Bytes to){
    // Check if the caller is the Expload Auction
    AssertIsAuction();

    // Get the asset with the specified id
    var asset = GetXGAsset(id);
    // Get the former asset owner's address
    var oldOwner  = asset.Owner;

    // Check if the asset actually exists
    // (if it has an owner)
    if(oldOwner == Bytes.VOID_ADDRESS){
        Error.Throw("This asset doesn't exist.");
    }

    // Change the asset owner
    asset.Owner = to;
    // Put the modified asset into the main storage
    _XGAssets[id] = asset;

    // Now we will change the user's assets storage

    // Deleting from the former owner's storage

    // Get the former owner's assets amount
    var oldOwnerAssetCount = _XGUsersAssetCount.GetOrDefault(oldOwner, 0);
    // Get the asset's serial number
    var oldOwnerSerialNumber = _SerialNumbers.GetOrDefault(id, 0);
    // Get the last asset in the former owner's storage
    var lastAsset = _XGUsersAssetIds.GetOrDefault(GetUserAssetKey(oldOwner, oldOwnerAssetCount-1), 0);
    // Put the last asset instead of the asset we're transferring
    _XGUsersAssetIds[GetUserAssetKey(oldOwner, oldOwnerSerialNumber)] = lastAsset;
    // Delete the last asset (as it is now in the place of the transferred asset)
    _XGUsersAssetIds[GetUserAssetKey(oldOwner,oldOwnerAssetCount-1)] = 0;
    // Decrease the asset count
    _XGUsersAssetCount[oldOwner] = oldOwnerAssetCount - 1;

    // Add to the new owner's storage

    // Get a new serial number
    var newSerialNumber = _XGUsersAssetCount.GetOrDefault(to, 0);
    // Put the id into the new owner's storage
    _XGUsersAssetIds[GetUserAssetKey(to, newSerialNumber)] = id;
    // Update the new owner's asset amount
    _XGUsersAssetCount[to] = newSerialNumber + 1;

    // Update the assets serial numbers
    _SerialNumbers[lastAsset] = oldOwnerSerialNumber;
    _SerialNumbers[id] = newSerialNumber;

    // Generate an event
    Log.Event("TransferXG", asset);
}
```

</details>

## Getting Data from the Program

### Get Asset Data by its `id`

`GetXGAssetData(long id) returns Asset asset` - returns the asset with the specified `id`.

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method implementation (the comments are changed for the sake of
convenience)

```c#
// A private method for getting an asset object from the storage
// (used in many different methods)
private Asset GetXGAsset(long id){
    return _XGAssets.GetOrDefault(id, new Asset());
}

public Asset GetXGAssetData(long id){
    // Calling the private method
    return GetXGAsset(id);
}
```

</details>

### Get the Asset Owner by the Asset's `id`

`GetXGAssetOwner(long id) returns Bytes owner` — returns the address of the
current asset owner with the specified `id`

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method implementation (the comments are changed for the sake of convenience)

```c#
public Bytes GetXGAssetOwner(long id){
    // Using the private method as described above
    return GetXGAsset(id).Owner;
}
```

</details>

### Get the Asset's `classId` by its `id`

`GetXGAssetClassId(long id) returns Bytes classId` — returns the `classId` 
of asset with specified `id`

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method implementation (the comments are changed for the sake of convenience)

```c#
public Bytes GetXGAssetClassId(long id){
    // Using the private method as described above
    return GetXGAsset(id).ItemClassId;
}
```

</details>

### Get the Amount of the User's Asset

`GetUsersXGAssetCount(Bytes address) returns long assetCount` — returns the amount 
of assets owned by the user with the specified `address`

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method implementation (the comments are changed for the sake of convenience)

```c#
public long GetUsersXGAssetCount(Bytes address){\
    // Get the value from mapping
    return _XGUsersAssetCount.GetOrDefault(address, 0);
}
```

</details>

### Get Asset's `id` by its Serial Number

`GetUsersXGAssetId(Bytes address, long number) returns long id` — returns the `id` of the asset with the serial number `number` and owned by the user with the specified `address`

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method implementation (the comments are changed for the sake of convenience)

```c#
// Private method for getting the required id
// (Also used in other methods)
private long _getUsersXGAssetId(Bytes address, long number){
    // The serial number can't be bigger than asset amount
    if(number >= _XGUsersAssetCount.GetOrDefault(address, 0)){
        Error.Throw("This asset doesn't exist!");
    }

    // Get the required id from the user's asset storage
    var key = GetUserAssetKey(address, number);
    return _XGUsersAssetIds.GetOrDefault(key, 0);
}

public long GetUsersXGAssetId(Bytes address, long number){
    // Call the private method
    return _getUsersXGAssetId(address, number);
}
```

</details>

### Request all the Assets Owned by the User

`GetUsersAllXGAssetsData(Bytes address) returns Asset[] inventory` — returns an array of assets owned by the user with the specified `address`.

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method implementation (the comments are changed for the sake of convenience)

```c#
public Asset[] GetUsersAllXGAssetsData(Bytes address){
    // Get the user's assets amount
    int amount = (int)_XGUsersAssetCount.GetOrDefault(address, 0);
    // Create an empty array
    var result = new Asset[amount];

    // Fill it with assets
    for(int num = 0; num < amount; num++){
        // Get asset's id using private method described above,
        // then get the asset using id
        result[num] = GetXGAsset(_getUsersXGAssetId(address, num));
    }
    return result;
}

```

</details>

## Deploy and Implementation Setup

Now that we have the understanding of the asset structure and interface, let's move on to the Pravda program setup.
We will use [XGold implementation example](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/TradableXGAsset.cs).

### Pravda Program Deployment

```sh
# Clone the repository
git clone https://github.com/expload/auction

# Move to the implementation folder
cd auction/TradableAsset/source/XG

# Let's generate a Pravda wallet - wallet.json — we will use it to deploy pravda gen address -o wallet.json

# ! IMPORTANT !
# You need to replenish the wallet before moving on:
# https://faucet.dev.expload.com/ui

# Now let’s deploy the program to test-net:
dotnet publish -c Deploy

# program-wallet.json should appear in the directory.
# This is the 'admin wallet' - you will need it for
# program configuration.
```

The address from `program-wallet.json` should be forwarded to Expload’s team member, who will add the game to the database.

### Program Configuration

The commands are called from the deploy directory 
(`auction/TradableAsset/source/XG`)

```sh
# We need to set up the Expload Auction address variable
# (so that the auction will have the right to transfer assets)

# You can ask an Expload team member for the current Auction address
# For the purposes of this tutorial, we will consider that the Expload Auction has address "A",
# program-wallet.json has address "B"

# ! IMPORTANT !
# Remember to replenish your program-wallet.json
# And insert a valid addresses before the execution

echo "push xA push \"SetAuction\" push xB push 2 pcall" | pravda compile asm | pravda broadcast run -w program-wallet.json -l 9000000
```

```sh
# You can also specify the fee charged by 
# your game when selling items on the Expload Auction
echo "push {percent} push {gameId} push true push \"SetGameCommission\" push xA push 4 pcall" | pravda compile asm | pravda broadcast run -w program-wallet.json -l 9000000

# Where {percent} is the fee percent.
# {gameId} is id (:long) your game's `gameId`, 
# given by an Expload team member.

# The Expload Auction has its inner fee too.
# E.g. if your game has a 5% feee, and the Expload Auction
# has a 5% fee too, there will be a total of 10% fee:
# If someone sells an item for 100 XG, they will receive 90 XG
```

Congrats! Your program is set up and ready to run.  
  
The only thing you need to do now for your game to appear on the Expload Auction is to create a [meta-server](meta-data.md).

## Developing Your Own Implementation

Expload supports custom asset implementations. 
Implementation must be inherited from either 
[ITradableXGAsset interface](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/ITradableXGAsset.cs) 
or [ITradableXPAsset interface](https://github.com/expload/auction/blob/master/TradableAsset/source/XP/ITradableXPAsset.cs) 
(depending on the asset type).

There is a [nuget-package](https://www.nuget.org/packages/Expload.Standards.TradableAsset/) including both interfaces.

# TradableAsset - standard and implementation example

## Overview

For your game to be compatible with Expload Auction and Inventory all 
in-game items (or `assets`) should be stored in a Pravda-program 
inheriting either [ITradableXGAsset interface](https://github.com/expload/Auction/blob/master/TradableAsset/source/XG/ITradableXGAsset.cs) either 
[ITradableXPAsset interface](https://github.com/expload/Auction/blob/master/TradableAsset/source/XP/ITradableXPAsset.cs).  
Interface `ITradableXGAsset` is suitable for `XGold Assets` - assets 
which can only be sold for XGold on Expload Auction. Similarly, 
ITradableXPAsset is suitable for `XPlatinum Assets` which can only be 
sold for XPlatinum. Those interfaces' logic and structure is identical, 
the only difference is naming (e.g. `GetXGAsset` and `GetXPAsset` have the same purpose).  
  
Let's look into those interfaces by analyzing an [example of XGold Asset implementation](https://github.com/expload/Auction/blob/master/TradableAsset/source/XG/TradableXGAsset.cs).  
  
> Code snippets from the implementation are included for deeper 
> understanding of asset's inner logic, reading the snippets is not 
> necessary. If your game doesn't need any specific mechanics of assets 
> emission or transference, you can just use the example implementation 
> linked above in your game and use it like an API. Check out the asset's 
> structure and method interface below and move on to [Deploy and Implementation Setup](#Deploy-and-Implementation-Setup).

## Asset's structure

Each asset has fields:

`long Id` (or `id`) - blockchain-id of the asset. Doesn't have any in-game 
meaning. Is only used inside Pravda programs as an unique asset 
identificator.

`Bytes Owner` - address of the current asset owner.

`Bytes ItemClassId` (or `classId`) - in-game id  of a specific asset 
class. _For example, two in-game swords called "Deathbringer" have 
different upgrades - one was sharpened by blacksmith and the other one was 
enhanced by wizard. It means that they have the same `classId` as they 
are the same class of item, but their instances differ._ Assets with equal 
`classId` will be put in a single Expload Auction tab.

`Bytes ItemInstanceId` (or `instanceId`) - in-game id of a specific item 
instance. _Fox example, two in-game swords called "Deathbringer" have 
different upgrades - one was sharpened by blacksmith and the other one was 
enhanced by wizard. It means that they have different `instanceId` as 
despite being the same item class their instances differ._

## Asset operations

### Asset emission

`EmitXGAsset(Bytes owner, Bytes classId, Bytes  instanceId) returns long 
id` - emits an assets with specified parameters and gives it to user with 
address `Bytes owner`. Returns a unique asset `id`. Can only be called by 
Pravda program owner

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method's implementation (comments are changed
for convenience' sake)

```c#
public long EmitXGAsset(Bytes owner, Bytes classId, Bytes instanceId){
    // Checking if the method caller
    // is the program's owner
    AssertIsGameOwner();

    // _lastXGId - global variable storing 
    // the last id given to an asset
    // We increase it by 1
    var id = ++_lastXGId;

    // Create asset object
    var asset = new Asset(id, owner, classId, instanceId);

    // Add the asset to _XGAssets mapping - main asset storage
    _XGAssets[id] = asset;

    /*
    Apart from the main asset storage, there is
    a special asset storage for each user, making
    it  really easy to get all the asset owned
    by a particular user
    */

    // Add the asset to user storage

    // Get the current amount of assets owned by the user
    // from _XGUsersAssetCount mapping
    var assetCount = _XGUsersAssetCount.GetOrDefault(owner, 0);

    // Store the asset id in user storage
    _XGUsersAssetIds[GetUserAssetKey(owner, assetCount)] = id;
    // Increase user's asset count by 1
    _XGUsersAssetCount[owner] = assetCount + 1;
    // Add asset serial number (its key in user storage mapping) 
    // to the serial number storage
    // (so we don't have to iterate through all user's assets)
    _SerialNumbers[id] = assetCount;

    // Emit an event
    Log.Event("EmitXG", asset);

    // Return asset's unique id
    return id;
}
```

</details>

### Asset transference

`TransferXGAsset(long id, bytes to)` - transfers the asset with specified `id` 
to address `to`. Can only be called by Expload Auction.

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method's implementation (comments are changed
for convenience' sake)

```c#
public void TransferXGAsset(long id, Bytes to){
    // Check if the caller is Expload Auction
    AssertIsAuction();

    // Get the asset with specified id
    var asset = GetXGAsset(id);
    // Get old asset owner's address
    var oldOwner  = asset.Owner;

    // Check if this asset actually exists
    // (if it has an owner)
    if(oldOwner == Bytes.VOID_ADDRESS){
        Error.Throw("This asset doesn't exist.");
    }

    // Change the asset owner
    asset.Owner = to;
    // Put the modified asset into the main storage
    _XGAssets[id] = asset;

    // Now we are to change users' assets storage

    // Deleting from old owner's storage

    // Get old owner's assets amount
    var oldOwnerAssetCount = _XGUsersAssetCount.GetOrDefault(oldOwner, 0);
    // Get asset's serial number
    var oldOwnerSerialNumber = _SerialNumbers.GetOrDefault(id, 0);
    // Get the last asset in old owner's storage
    var lastAsset = _XGUsersAssetIds.GetOrDefault(GetUserAssetKey(oldOwner, oldOwnerAssetCount-1), 0);
    // Put the last asset instead of the asset we're transferring
    _XGUsersAssetIds[GetUserAssetKey(oldOwner, oldOwnerSerialNumber)] = lastAsset;
    // Delete the last asset (as it is now on the place of transferred asset)
    _XGUsersAssetIds[GetUserAssetKey(oldOwner,oldOwnerAssetCount-1)] = 0;
    // Decrease the asset count
    _XGUsersAssetCount[oldOwner] = oldOwnerAssetCount - 1;

    // Add to new owner's storage

    // Get the bew serial number
    var newSerialNumber = _XGUsersAssetCount.GetOrDefault(to, 0);
    // Put the id into new owner's storage
    _XGUsersAssetIds[GetUserAssetKey(to, newSerialNumber)] = id;
    // Update new owner's asset amount
    _XGUsersAssetCount[to] = newSerialNumber + 1;

    // Update assets serial numbers
    _SerialNumbers[lastAsset] = oldOwnerSerialNumber;
    _SerialNumbers[id] = newSerialNumber;

    // Emit an event
    Log.Event("TransferXG", asset);
}
```

</details>

## Getting data from the program

### Get asset data by its `id`

`GetXGAssetData(long id) returns Asset asset` - returns the asset with 
specified `id`.

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method's implementation (comments are changed
for convenience' sake)

```c#
// Private method for getting asset object from storage
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

### Get asset owner  by asset's `id`

`GetXGAssetOwner(long id) returns Bytes owner` - returns the address of 
current owner of asset with specified `id`

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method's implementation (comments are changed
for convenience' sake)

```c#
public Bytes GetXGAssetOwner(long id){
    // Using the private method described above
    return GetXGAsset(id).Owner;
}
```

</details>

### Get asset's `classId` by its `id`

`GetXGAssetClassId(long id) returns Bytes classId` - returns the `classId` 
of asset with specified `id`

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method's implementation (comments are changed
for convenience' sake)

```c#
public Bytes GetXGAssetClassId(long id){
    // Using the private method described above
    return GetXGAsset(id).ItemClassId;
}
```

</details>

### Get the amount of user's asset

`GetUsersXGAssetCount(Bytes address) returns long assetCount` - returns the amount 
of assets owned by user with specified `address`

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method's implementation (comments are changed
for convenience' sake)

```c#
public long GetUsersXGAssetCount(Bytes address){\
    // Get the value from mapping
    return _XGUsersAssetCount.GetOrDefault(address, 0);
}
```

</details>

### Get asset's `id` by its serial number

`GetUsersXGAssetId(Bytes address, long number) returns long id` - returns the `id` of asset 
with serial number `number` and owned by user with specified `address`

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method's implementation (comments are changed
for convenience' sake)

```c#
// Private method for getting the needed id
// (Also used in other methods)
private long _getUsersXGAssetId(Bytes address, long number){
    // Serial number can't be bigger than asset amount
    if(number >= _XGUsersAssetCount.GetOrDefault(address, 0)){
        Error.Throw("This asset doesn't exist!");
    }

    // Get the needed id from user's asset storage
    var key = GetUserAssetKey(address, number);
    return _XGUsersAssetIds.GetOrDefault(key, 0);
}

public long GetUsersXGAssetId(Bytes address, long number){
    // Call the private method
    return _getUsersXGAssetId(address, number);
}
```

</details>

### Get all assets owned by a particular user

`GetUsersAllXGAssetsData(Bytes address) returns Asset[] inventory` - returns an array of assets 
owned by a user with specified `address`.

<details>

<summary>Implementation example snippet</summary> 

Let's look into this method's implementation (comments are changed
for convenience' sake)

```c#
public Asset[] GetUsersAllXGAssetsData(Bytes address){
    // Get user's assets amount
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

Now, when we have an understanding of asset structure and interface, 
let's move on to the Pravda program setup.
We will use [XGold implementation example](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/TradableXGAsset.cs).

### Pravda program deploy

```sh
# Clone the repository
git clone https://github.com/expload/auction

# Move to the implementation folder
cd auction/TradableAsset/source/XG

# Let's generate a Pravda wallet - wallet.json - we will use it for deploy
pravda gen address -o wallet.json

# ! IMPORTANT !
# You need to top up wallet balance before moving on:
# https://faucet.dev.expload.com/ui

# Now let us deploy the program to test-net:
dotnet publish -c Deploy

# program-wallet.json should appear in the directory.
# This is the 'admin wallet' - you will need it for
# program configuration.
```

The address from `program-wallet.json` should be forwarded 
to Expload representative, so the game will be added to database.

### Program configuration

The commands are called from deploy directory 
(`auction/TradableAsset/source/XG`)

```sh
# We have to set up Expload Auction address variable
# (so that the auction will have the right to transfer assets)

# You can ask Expload representative for a current Auction address
# For this tutorial, we will consider that Expload Auction has address "A",
# program-wallet.json has address "B"

# ! IMPORTANT !
# Don't forget to top up the balance of program-wallet.json
# And insert valid addresses before execution

echo "push xA push \"SetAuction\" push xB push 2 pcall" | pravda compile asm | pravda broadcast run -w program-wallet.json -l 9000000
```

```sh
# You can also specify the commission taken by 
# your game when selling items on Expload Auction
echo "push {percent} push {gameId} push true push \"SetGameCommission\" push xA push 4 pcall" | pravda compile asm | pravda broadcast run -w program-wallet.json -l 9000000

# Where {percent} - is the commission in percents.
# {gameId} - id (:long) your game's `gameId`, 
# given by Expload representative.

# Expload Auction has its inner commission too.
# E.g. if your game has 5% commission, and Expload Auction
# has 5% commission too, there will be a total of 10% commission:
# If someone is selling item for 100 XG, he will get 90 XG
```

Congratulations! Your program is set up and ready to go.  
  
Now, the only thing you need to do for your game to appear on Expload auction 
is create a [meta-server](meta-data.md).

## Developing your own implementation

Expload supports custom asset implementations. 
Implementation must be inherited from either 
[ITradableXGAsset interface](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/ITradableXGAsset.cs) 
either [ITradableXPAsset interface](https://github.com/expload/auction/blob/master/TradableAsset/source/XP/ITradableXPAsset.cs) 
(depending on the asset type).

There is a [nuget-package](https://www.nuget.org/packages/Expload.Standards.TradableAsset/) including both interfaces.

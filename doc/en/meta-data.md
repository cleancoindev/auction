# Metadata

**IMPORTANT!**  
If you haven't yet set up your `TradableAsset` Pravda program, please read [The Tradable Asset Standard](tradable-asset-standard.md) first.  
  
The integration with Expload Auction also requires a meta server that provides Expload Auction with all the necessary asset data — names, descriptions, preview pictures, etc.

## Metadata Structure

Metadata has the following format:

```json
{
    "name": <itemName>,
    "description": <itemShortDescription>,
    "pictureUrl": <itemPictureURL>,
    "previewPictureUrl": <itemPreviewPictureURL>,
    "tags": <tagsData>
}
```

`name` — the asset name. 
_For example, "Toy Sword"_.  
`description` — a short asset description. 
_For example, "Give it back to me, I'll call my mom!"_.  
`pictureUrl` — a link to the asset picture. `.png`, transparent background, 
square orientation is preferred. Maximal size - 512px x 512 px.  
`previewPictureUrl` — a link to a smaller (preview) asset picture. 
The requirements are the same as for `pictureUrl`.  
`tags` — a dictionary with other asset data. 
_For example:_  

```json
{
    "name": "Toy Sword",
    "description": "Give it back to me, I'll call my mom!",
    "pictureUrl" : "some_url.com/toysword.png",
    "previewPictureUrl" : "some_url.com/toysword_small.png",
    "tags" :  {
        "rarity": ["legendary"],
        "class": ["warrior", "paladin", "berserk"]
    }
}
```

Data in `tags` can be used for searching and filtering assets on Expload Auction.   
  
As [previously described](tradable-asset-standard.md), each asset has two identifiers having an in-game meaning — `classId` and `instanceId`. The meta server should provide asset data using either identifier. `classId` metadata will be shown in the auction asset tabs, while `instanceId` metadata — when choosing specific auction lots and inventory items.
In other words, `classId` is more general information than `instanceId`. Data shown in class meta shouldn't be duplicated in instance meta: empty instance data fields will be replaced by class data fields and non-empty instance data fields will override class data fields.

## Meta Server

The game should have a meta server which has a public API for class- and instance-meta. 
There is an [implementation example](https://github.com/krylov-na/TestMetaServer) written in C#.

## Program Setup

When your meta server is ready, it is necessary to add information about it to your `TradableAsset` Pravda program. Modify the methods `GetClassIdMeta` and `GetInstanceIdMeta` in such a way that once these methods are given a `classId` or an `instanceId`, they return a link to the required data on your [meta server](#Meta-server).  
  
After modifying the code you have to either [deploy your program](tradable-asset-standard.md) or, in the event that you have done it before, use the following commands:

```sh
# ! IMPORTANT ! 
# For custom implementations this will
# only work for development using PravdaProgramTemplate:
# https://bit.ly/2ARW0uR

dotnet publish -c Update
```

## Auction

Once you have completed all the steps as described above and your game has been assigned a `GameId` by an Expload team member, you are ready to work with the Auction!

Your game will be displayed in the Auction within the Expload app. You can create an additional interface for the auction inside your game, using [the Auction source code](https://github.com/expload/auction/blob/master/Auction/source/Auction.cs).
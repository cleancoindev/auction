# Meta data

**IMPORTANT!**  
If you haven't set up your `TradableAsset` Pravda program yet, 
read [Tradable Asset Standard](tradable-asset-standard.md) first.  
  
The integration with Expload Auction also requires a meta-server, 
providing Expload Auction with all necessary asset data - names, 
descriptions, preview pictures, etc.

## Meta data structure

Meta data has the following format:

```json
{
    "name": <itemName>,
    "description": <itemShortDescription>,
    "pictureUrl": <itemPictureURL>,
    "previewPictureUrl": <itemPreviewPictureURL>,
    "tags": <tagsData>
}
```

`name` - asset name. 
_For example, "Toy Sword"_.  
`description` - short asset description. 
_For example, "Give it back to me, I'll cal my mom!"_.  
`pictureUrl` - link to asset's picture. `.png`, transparent background, 
square orientation is preferred. Maximal size - 512px x 512 px.  
`previewPictureUrl` - link to a smaller (preview) asset picture. 
Requirements are the same as for `pictureUrl`.  
`tags` - dictionary with other asset data. 
_For example:_  

```json
{
    "name": "Toy Sword",
    "description": "Give it back to me, I'll cal my mom!",
    "pictureUrl" : "some_url.com/toysword.png",
    "previewPictureUrl" : "some_url.com/toysword_small.png",
    "tags" :  {
        "rarity": ["legendary"],
        "class": ["warrior", "paladin", "berserk"]
    }
}
```

Data in `tags` can be used for searching and filtering 
assets on Expload Auction.   
  
As [previously described](tradable-asset-standard.md), each asset has two identificators having an in-game meaning - `classId` and `instanceId`. Meta-server should provide asset data using any of those two identificators. `classId` meta data will be shown in auction asset tabs, `instanceId` meta data - when choosing specific 
auction lots and inventory items.
Another words, `classId` is a more general information then `instanceId`. 
Data shown in class meta shouldn't be duplicated in instance meta: 
empty instance data fields will be replaced by class data fields and 
non-empty instance data fields will override class data fields.

## Meta-server

The game should have a meta-server which has a public API for class- and instance-meta. 
There is an [implementation example](https://github.com/krylov-na/TestMetaServer) written in C#.

## Program setup

When your meta-server is ready it is necessary to add 
information about it to your `TradableAsset` Pravda program. 
Modify methods `GetClassIdMeta` and `GetInstanceIdMeta` so that
when those methods are given a `classId` or an `instanceId`, they return
a link to the needed data on your [meta server](#Meta-server).  
  
After modifying the code you have to either [deploy your program](tradable-asset-standard.md), 
or, if you've done it before, use following commands:

```sh
# ! IMPORTANT ! 
# For custom implementations this will
# only work for development using PravdaProgramTemplate:
# https://bit.ly/2ARW0uR

dotnet publish -c Update
```

## Auction

Если вы выполнили все предыдущие шаги, и представитель Expload присвоил вашей игре `GameId`, то вы готовы к работе с аукционом!

Ваша игра будет отображаться в аукционе в приложении Expload. Вы также можете сделать дополнительный интерфейс аукциона внутри своей игры, используя [исходный код аукциона](https://github.com/expload/auction/blob/master/Auction/source/Auction.cs).

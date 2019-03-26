#!/bin/bash

payer="none" # Payer wallet, pays for all the executions
auction="none" # Auction program wallet
test_asset="none" # Test game TradableAsset program wallet
xgold="none" # Current XGold program wallet
meta_server="none" # Test game meta server address
endpoint="http://localhost:8080/api/public" # Pravda endpoint

help="false" # Show help tooltip
game_add="false" # Flags if test asset should be added to auction

help_text="\n""Options:"
help_text+="\n"'-p --payer        Payer wallet, pays for all the executions'
help_text+="\n""-a --auction      Auction program wallet"
help_text+="\n""-t --test-asset   Test game TradableAsset program wallet"
help_text+="\n""-x --xgold        Current XGold program wallet"
help_text+="\n""-m --meta-server  Test game meta server base URL (without class/instance postfixes)"
help_text+="\n""-e --endpoint     Pravda endpoint, default: http://localhost:8080/api/public"
help_text+="\n""-g --game-add     Flags if test asset should be added to auction"
help_text+="\n""-h --help         Show this help message"

# Parse arguments
while [ "$1" != "" ]; do
    case $1 in
        -p | --payer )        shift
                              payer=$1
                              ;;
        -a | --auction )      shift
                              auction=$1
                              ;;
        -t | --test-asset )   shift
                              test_asset=$1
                              ;;
        -x | --xgold )        shift
                              xgold=$1
                              ;;
        -m | --meta-server )  shift
                              meta_server=$1
                              ;;
        -e | --endpoint )     shift
                              endpoint=$1
                              ;;
        -g | --game-add )     shift
                              game_add="true"
                              ;;
        -h | --help )         shift
                              help="true"
                              ;;
        * )                   echo "Unknown argument: $1"
                              echo -e "${help_text}"
                              exit 1
    esac
    shift
done

# Show help tooltip if needed
if [[ $help == "true" ]]; then
    echo -e "${help_text}"
    exit 0
fi

# Check if all needed arguments are provided
if [[ $payer == "none" ]] || [[ $auction == "none" ]] || \
[[ $test_asset == "none" ]] || [[ $xgold == "none" ]] || [[ $meta_server == "none" ]]; then
    echo "Not enough arguments!"
    exit 1
fi

# Get program addresses
auction_address=$( cat $auction | grep -o ":\"\w*\"" | head -n1 )
auction_address=${auction_address:2:64}

test_asset_address=$( cat $test_asset | grep -o ":\"\w*\"" | head -n1 )
test_asset_address=${test_asset_address:2:64}

xgold_address=$( cat $xgold | grep -o ":\"\w*\"" | head -n1 )
xgold_address=${xgold_address:2:64}

# Deploy auction program

# Compile dotnet solution
echo "Compiling Auction program..."
publish_log=$( dotnet publish Auction/source/Auction.sln )
if [[ $publish_log == *"error MSB"* ]]; then
    echo "dotnet publish failed"
    exit 1
fi

# Auction dotnet package bin route
auction_compiled="Auction/source/bin/Auction.pravda"

# Check if auction program was previously deployed
echo "Checking if Auction has been deployed before..."
auction_deployed=$( echo "push x$auction_address pexist" | pravda compile asm | pravda broadcast run -w $payer -l 10000 -e $endpoint )

# Update if already deployed
if [[ $auction_deployed == *"bool.true"* ]]; then
    echo "Auction already deployed, updating the program"
    update_log=$( pravda broadcast update -i $auction_compiled -w $payer -p $auction -l 9000000 -e $endpoint )
    if [[ $update_log == *"Exception in"* ]]; then
        echo "Failed to update Auction"
        exit 1
    else
        echo "Auction program successfully updated"
    fi
# Deploy if not yet deployed
else
    echo "Auction not deployed yet, deploying"
    deploy_log=$( pravda broadcast deploy -i $auction_compiled -w $payer -p $auction -l 9000000 -e $endpoint )
    if [[ $deploy_log == *"Exception in"* ]]; then
        echo "Failed to deploy Auction"
        exit 1
    else
        echo "Auction program successfully deployed"
    fi
fi

# Deploy test TradableAsset program

# Edit meta URL
meta_server_escaped=$(echo $meta_server | sed 's/\//\\\//g')
command="0,/https:\/\/some_url\//s/https:\/\/some_url\//${meta_server_escaped}class-meta\//"
sed -i $command TradableAsset/source/XG/TradableXGAsset.cs
command="0,/https:\/\/some_url\//s/https:\/\/some_url\//${meta_server_escaped}instance-meta\//"
sed -i $command TradableAsset/source/XG/TradableXGAsset.cs

# Compile dotnet solution
echo "Compiling TradableAsset program..."
publish_log=$( dotnet publish TradableAsset/source/XG/TradableXGAsset.csproj )
if [[ $log == *"error MSB"* ]]; then
    echo "dotnet publish failed"
    exit 1
fi

# TradableAsset dotnet package bin route
test_asset_compiled="TradableAsset/source/XG/bin/TradableXGAsset.pravda"

# Check if TradableAsset program was previously deployed
echo "Checking if TradableAsset has been deployed before..."
test_asset_deployed=$( echo "push x$test_asset_address pexist" | pravda compile asm | pravda broadcast run -w $payer -l 10000 -e $endpoint )

# Update if already deployed
if [[ $test_asset_deployed == *"bool.true"* ]]; then
    echo "TradableAsset already deployed, updating the program..."
    update_log=$( pravda broadcast update -i $test_asset_compiled -w $payer -p $test_asset -l 9000000 -e $endpoint )
    if [[ $update_log == *"Exception in"* ]]; then
        echo "Failed to update TradableAsset"
        exit 1
    else
        echo "TradableAsset program successfully updated"
    fi
# Deploy if not yet deployed
else
    echo "TradableAsset not deployed yet, deploying..."
    deploy_log=$( pravda broadcast deploy -i $test_asset_compiled -w $payer -p $test_asset -l 9000000 -e $endpoint )
    if [[ $deploy_log == *"Exception in"* ]]; then
        echo "Failed to deploy TradableAsset"
        exit 1
    else
        echo "TradableAsset program successfully deployed"
    fi
fi

# Set Auction address in TradableAsset
echo "Setting Auction address in TradableAsset..."
set_auction_log=$( echo "push x$auction_address push \"SetAuction\" push x$test_asset_address push 2 pcall" | pravda compile asm | pravda broadcast run -w $test_asset --watt-payer-wallet $payer -l 100000 -e $endpoint )

if [[ $set_auction_log == *"Exception in"* ]]; then
    echo "Failed to set Auction address"
    exit 1
else
    echo "Auction address successfully set"
fi

# Set XGold adress in Auction
echo "Setting XGold address in Auction..."
set_xgold_log=$( echo "push x$xgold_address push \"SetXGAddress\" push x$auction_address push 2 pcall" | pravda compile asm | pravda broadcast run -w $auction --watt-payer-wallet $payer -l 100000 -e $endpoint )

if [[ $set_xgold_log == *"Exception in"* ]]; then
    echo "Failed to set XGold address"
    exit 1
else
    echo "XGold address successfully set"
fi

# Add game to Auction if needed
if [[ $game_add == "true" ]]; then
    echo "Adding game to Auction..."
    add_game_log=$( echo "push x$test_asset_address push true push \"AddGame\" push x$auction_address push 3 pcall" | pravda compile asm | pravda broadcast run -w $auction --watt-payer-wallet $payer -l 100000 -e $endpoint )
    if [[ $add_game_log == *"Exception in"* ]]; then
        echo "Failed to add game to Auction"
        exit 1
    else
        id=$( echo $add_game_log | grep -o -P "\"stack\"\s:\s\[\s\"int64.\d*\"\s\]" | grep -o -P "\"int64.\d*" )
        id=${id:7}
        echo "Game was successfully added to Auction, id: $id"
    fi
fi

echo "Finished deploying and setting up marketplace"

exit 0
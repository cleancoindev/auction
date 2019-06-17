#!/bin/bash

payer="none" # Payer wallet, pays for all the executions
auction="none" # Auction program wallet
test_asset_address="none" # Test game TradableAsset program wallet
endpoint="http://localhost:8080/api/public" # Pravda endpoint

help="false" # Show help tooltip
is_xgold="none"

help_text="\n""Options:"
help_text+="\n"'-p --payer        Payer wallet, pays for all the executions'
help_text+="\n""-a --auction      Auction program wallet"
help_text+="\n""-t --test-asset-address   Address of the TradableAsset game"
help_text+="\n""--is-xgold        true, if this TradableAsset address is address of the TradableXGAsset"
help_text+="\n""-e --endpoint     Pravda endpoint, default: http://localhost:8080/api/public"
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
        -t | --test-asset-address )   shift
                              test_asset_address=$1
                              ;;
        --is-xgold )          shift
                              is_xgold="true"
                              ;;
        -e | --endpoint )     shift
                              endpoint=$1
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
   [[ $test_asset_address == "none" ]] || [[ $is_xgold == "none" ]]; then
    echo "Not enough arguments!"
    exit 1
fi

# Get program addresses
auction_address=$( cat $auction | jq -r '.address' )

# Add game to Auction if needed
echo "Adding game $test_asset_address to Auction..."
add_game_log=$( echo "push x$test_asset_address push $is_xgold push \"AddGame\" push x$auction_address push 3 pcall" | pravda compile asm | pravda broadcast run -w $auction --watt-payer-wallet $payer -l 100000 -e $endpoint )
if [[ $add_game_log == *"Exception in"* ]]; then
    echo "Failed to add game to Auction"
    exit 1
else
    id=$( echo $add_game_log | jq '.executionResult.success.stack[0]' )
    echo "Game was successfully added to Auction, id: $id"
fi

echo "Finished adding game to marketplace"

exit 0

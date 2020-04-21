# Save game manager
This is a save game manager for Advanced tactics gold. What it does it syncs a (savegame) directory between you, Azure blob storage and any other players.

## How does it work?
First of all you need an Azure blob storage (which is basically an online file storage):
- https://docs.microsoft.com/nl-nl/azure/storage/blobs/storage-blob-create-account-block-blob?tabs=azure-portal

Once this is setup then you get the connection string and add this to your appsettings.json.

In the appsettings.json there are a few other settings you must check. First of all you can add one or more games to the GameType section. Here you can define all the games you wish to play PBEM style, their respective savegame directory and any icon if you so wish. 

You need to fill in a playername in the appsettings.json as well, this should be unique within the group you're playing with.

Once this is done you can start the application. Every time you press the sync button in the application it will compare your multiplayer savegames to the Azure blob storage and tries to sync any changes.

![alt text](https://i.imgur.com/6D7nZVv.png "Example screen")

## How to play?
First you need to create a game in Advanced Tactics or any game that supports PBEM or hotseat and save it, remember the order of human players. Do your first turn (as first player). 

Then start up the savegame manager, go to file -> new game. 

Give the game a name and select the savegame that belongs to the game. 

Add a list of players, here it is VERY important to fill in the player names exactly like the players have added them to their appsettings.json. 

![alt text](https://i.imgur.com/2lKRAC4.png "Create a new game")

If you press save it will create a 'gamename'.json file in the directory /data. This keeps track of what savegames are multiplayer and will be synced.

The game should now appear in your list after saving and you can press sync to upload it.

When someone tells you it's your turn you open the application, and press Sync. The tool should show you the files that are new and the ones that are changes (Downloaded). 
You do your turn and as soon as you're done and save the game (preferably override the old savegame), then you press sync again and the status should say Uploaded. Now you can give the next player a message that it is his turn.

## Download
Check the releases page for downloads and download PBEMGameManager.zip.

[Download](https://github.com/noldofinwe/SavegameManager/releases)

Be aware this is made in .NET Core 3.1 and might require you to download the .NET core runtime

[Download .NET core runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1)


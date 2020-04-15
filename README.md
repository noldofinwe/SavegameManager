# Save game manager
This is a save game manager for Advanced tactics gold. What it does it syncs a (savegame) directory between you, Azure blob storage and any other players.

## How does it work?
First of all you need an Azure blob storage:
- https://docs.microsoft.com/nl-nl/azure/storage/blobs/storage-blob-create-account-block-blob?tabs=azure-portal

Then you get the connection string and add this to your appsettings.json, in there you can select what directory to sync as well.

Every time you press the sync button in the application, it will check what files you have in the directory and compares them to the files in the blob storage. All the files that are newer locally and different (MD5 check) will be uploaded and all the files that are older and different will be downloaded.


## How to play?
When someone tells you it's your turn you open the application, and press Sync. The tool should show you the files that are new and the ones that are changes (Downloaded). 
You do your turn and as soon as you're done and save the game (preferably override the old savegame), then you press sync again and the status should say Uploaded. Now you can give the next player a message that it is his turn.


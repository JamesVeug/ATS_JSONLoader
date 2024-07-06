# JSONLoader

This is a simple mod for Against the Storm that allows you to load JSON files from your computer into the game.
These files can add new content or change existing content.

NO CODE REQUIRED

Currently only supports Goods


## Importing

To load a JSON file into the game you need to place the file in the plugins folder of BepInEx. 
- Using Thunderstore: `%appdata%\Thunderstore Mod Manager\DataFolder\AgainstTheStorm\profiles\Default\BepInEx\plugins`
- Manual Install: `AgainstTheStorm\BepInEx\plugins`

When you start the game the files will be loaded and any errors will show in console.

To reload while in game press `F5` and the game will reload the files.


## Exporting

JSONLoader allows you to export all of the game data to your computer so you can compare or edit as you need.

To Export using Thunderstore go into the configs tab and change `Export` to `true` then restart the game.


## Goods

```json
{
  "name": "Pizza",
  "icon": "pizza.png",
  "displayName": "Pizza",
  "description": "Delicious New York style pizza",
  "shortDescription": "Delicious New York style pizza",
  "category": "Food",
  "eatable": true,
  "canBeBurned": false,
  "burningTime": 1,
  "eatingFullness": 2.5,
  "tradingBuyValue": 2.3,
  "tradingSellValue": 10,
  "allTradersSellingThisGood": true,
  "allTradersBuyThisGood": true
}
```
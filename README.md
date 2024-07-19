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

Goods are items that can be traded, sold, eaten or burned. (Wood, planks, coal... etc).

To create new goods use the below as an example and name the file `*_good.json`. Example: `pizza_good.json`

```json
{
    "guid": "MyMod",
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

To replace existing goods just use the GUID and name of the good you want to change. If you're changing a vanilla good you can use the GUID ``.

Go to https://hoodedhorse.com/wiki/Against_the_Storm/Resources for a list of all goods in the game. and use the ID of the good as the `name` in the .json file.

```json
{
    "name": "[Mat Processed] Planks",
    "icon": "planks.png",
    "burningTime": 10
}
```


## Races

Ids: `Beaver, Foxes, Harpy, Human, Lizard`

Creating new races not yet supported.

To edit an existing Race use the below as an example with the name using the id of the Race. 

The sounds are optional. Remove them if you don't want them.


```json
{
  "name": "Beaver",
  "icon": "Beaver.png",
  "roundIcon": "BeaverRound.png",
  "widePortrait": "BeaverWide.png",
  "avatarClickSounds": {
    "PositiveSounds": {
      "sounds": [
        {
          "soundPath": "PeasantWhat1.wav"
        }
      ]
    },
    "NegativeSounds": {
      "sounds": [
        {
          "soundPath": "PeasantWhat1.wav"
        }
      ]
    },
    "NeutralSounds": {
      "sounds": [
        {
          "soundPath": "PeasantWhat1.wav"
        }
      ]
    }
  },
  "femalePickSounds": {
    "PositiveSounds": {
      "sounds": [
        {
          "soundPath": "PeasantReady1.wav"
        }
      ]
    },
    "NegativeSounds": {
      "sounds": [
        {
          "soundPath": "PeasantReady1.wav"
        }
      ]
    },
    "NeutralSounds": {
      "sounds": [
        {
          "soundPath": "PeasantReady1.wav"
        }
      ]
    }
  },
  "malePickSounds": {
    "PositiveSounds": {
      "sounds": [
        {
          "soundPath": "PeasantReady1.wav"
        }
      ]
    },
    "NegativeSounds": {
      "sounds": [
        {
          "soundPath": "PeasantReady1.wav"
        }
      ]
    },
    "NeutralSounds": {
      "sounds": [
        {
          "soundPath": "PeasantReady1.wav"
        }
      ]
    }
  },
  "femaleChangeProfessionSounds": {
    "PositiveSounds": {
      "sounds": [
        {
          "soundPath": "PeasantYes1.wav"
        }
      ]
    },
    "NegativeSounds": {
      "sounds": [
        {
          "soundPath": "PeasantYesAttack1.wav"
        }
      ]
    },
    "NeutralSounds": {
      "sounds": [
        {
          "soundPath": "PeasantYesAttack3.wav"
        }
      ]
    }
  },
  "maleChangeProfessionSounds": {
    "PositiveSounds": {
      "sounds": [
        {
          "soundPath": "PeasantYes1.wav"
        }
      ]
    },
    "NegativeSounds": {
      "sounds": [
        {
          "soundPath": "PeasantYesAttack1.wav"
        }
      ]
    },
    "NeutralSounds": {
      "sounds": [
        {
          "soundPath": "PeasantYesAttack3.wav"
        }
      ]
    }
  }
}
```
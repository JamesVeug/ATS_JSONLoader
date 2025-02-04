# JSONLoader

This is a simple mod for Against the Storm that allows you to load JSON files from your computer into the game.
These files can add new content or change existing content.

`NO CODING REQUIRED`

To add/modify content you just need to create a new file in a particular folder with the extension `.json` (Also known as a JSON files) and changing little bits to get the desired result.

Example: `Meatlovers_good.json`

`_good` defines the type of content you are adding/modifying.


## Schemas

![alt text](https://github.com/JamesVeug/ATS_JSONLoader/blob/master/Github/SchemaExample.png?raw=true "Schema Example")

Below are online forms that will allow you to create/edit the JSON files and save it as a file to your computer to avoid accidentally writing .json files incorrectly.    

General
- Goods: https://tinyurl.com/yvez6x6e
- Difficulties: https://tinyurl.com/muahb5z4
- MetaRewards: https://tinyurl.com/bdfm4tjm
- Races (excluding sounds): https://tinyurl.com/mrmmbjae
 
Recipes:
- WorkshopRecipe: https://tinyurl.com/tpsy6khx
- CampRecipe: https://tinyurl.com/3tz89mv4
- CollectorRecipe: https://tinyurl.com/zzdbzset
- FarmRecipe: https://tinyurl.com/yyeek2wj
- FishingHutRecipe: https://tinyurl.com/49avjxn4
- GathererHutRecipe: https://tinyurl.com/uzrj6atb
- InstitutionRecipe: https://tinyurl.com/4ar88jdc
- RainCatcherRecipe: https://tinyurl.com/54k5hasp

Buildings
- HouseBuilding: https://tinyurl.com/9bmhuezr
- InstitutionBuilding: https://tinyurl.com/yc3pm7bm
- DecorationBuilding: https://tinyurl.com/36p4abwz
- WorkshopBuilding: https://tinyurl.com/57awubuy

NOTE: Raw JSON files can be found in the `Schemas` folder of this repository.


### How to use the forms

1. Click the link to the form you want to use.
2. Fill in the form with as much data as you can
3. Select all the .json under `Update Form` and copy it.
4. Open a text editor like Notepad or Notepad++ and paste the data.
5. Save the file with the name you want and the extension `.json` Example: `MyGood_good.json`
6. Place the file in the plugins folder of BepInEx. See #Importing for more information.


## Importing

To load a JSON file into the game you need to place the file in the plugins folder of BepInEx. 
- Using Thunderstore: `%appdata%\Thunderstore Mod Manager\DataFolder\AgainstTheStorm\profiles\Default\BepInEx\plugins`
- Manual Install: `AgainstTheStorm\BepInEx\plugins`

When you start the game the files will be loaded and any errors will show in console.

To reload while in game press `F5` and the game will reload the files, or you can modify this in the key bindings tab of Against the Storms options menu.

- Note: Removing a .json file will not update the game but changing the .json file and images it references will update in the game. 


## Exporting

JSONLoader allows you to export all supported data of the game to your computer as images and `.json` files so you can copy+paste, compare or edit as you need.

To trigger an Export:
- Using Thunderstore go into the configs tab and change `Export` to `true` then restart the game.
- Using API go into the games mod menu and change `Export` to `true` then restart the game.
- Manually go into the BepInEx/configs folder, find JSONLoader and change `Export` to `true` then restart the game.

The exported files will be in the `%userprofile%\AppData\LocalLow\Eremite Games\Against the Storm\JSONLoader\Exported` folder.


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

Ids: `Beaver, Foxes, Harpy, Human, Lizard`, `Frog`

Creating new races not yet supported via .json yet.

To edit an existing Race use the below as an example with the name using the id of the Race. 

The sounds are optional. Remove them if you don't want them.


```json
{
  "name": "Beaver",
  "icon": "Beaver.png",
  "roundIcon": "BeaverRound.png",
  "widePortrait": "BeaverWide.png",
  "tag": "[Tag] Beaver",
  "isEssential": true,
  "order": 1,
  "baseSpeed": 1.8,
  "initialResolve": 10,
  "minResolve": 0,
  "maxResolve": 50,
  "resolvePositveChangePerSec": 0.15,
  "resolveNegativeChangePerSec": 0.12,
  "resolveNegativeChangeDiffFactor": 0.1,
  "reputationPerSec": 0.00013,
  "minPopulationToGainReputation": 1,
  "maxReputationFromResolvePerSec": 0.025,
  "minResolveForReputationTreshold": 30,
  "maxResolveForReputationTreshold": 50,
  "reputationTresholdIncreasePerReputation": 2,
  "resolveToReputationRatio": 0.1,
  "populationToReputationRatio": 0.7,
  "hungerTolerance": 6,
  "racialHousingNeed": "Beaver Housing",
  "needsInterval": 120,
  "needs": [
    "Any Housing",
    "Beaver Housing",
    "Biscuits",
    "Pickled Goods",
    "Clothes",
    "Leasiure",
    "Education",
    "Luxury"
  ],
  "characteristics": [
    {
      "buildingTag": "Wood",
      "villagerPerkEffect": "Proficiency",
      "globalEffect": "",
      "buildingPerk": ""
    },
    {
      "buildingTag": "Tech",
      "villagerPerkEffect": "Comfortable Job",
      "globalEffect": "",
      "buildingPerk": ""
    },
    {
      "buildingTag": "Hearth_Beavers",
      "villagerPerkEffect": "",
      "globalEffect": "FuelConsumption_HearthEffect_Beaver",
      "buildingPerk": ""
    }
  ],
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
  "femalePickSounds": {},
  "malePickSounds": {},
  "femaleChangeProfessionSounds": {},
  "maleChangeProfessionSounds": {},
  "maleNames": [
    "Sir James",
    "Dr James"
  ],
  "femaleNames": [
    "Lady Jane",
    "Dr Jane"
  ]
}
```


# Custom Difficulty

Custom difficulties are the modifiers you can add to the game to make it harder.

Against the Storm has a sequence of difficulties that you can add to the game. These start with basic difficulty names but then become Prestige 1, Prestige 2, etc.

To add a new difficulty you need to create a new file ending with `_difficulty.json`. Example: `extrahunger_difficulty.json`

```json
{
  "guid": "myMod",
  "name": "extrahunger",
  "icon": "extraHunger.png",
  "displayName": "Extra Hunger",
  "description": "Villagers will get hungry faster",
  "prestigeLevel": 21,
  "copyModifiersFromPreviousDifficulties": true,
  "newModifiers": [
    {
      "shortDescription": "Famine outbreaks in your previous settlements have made the villagers particularly sensitive to food shortages. Every time villagers have nothing to eat during a break, they will gain two stacks of the Hunger effect instead of one.",
      "effect": "[Diff] Hunger Multiplier",
      "isEarlyEffect": true,
      "isShown": true,
      "inCustomMode": true
    }
  ]
}
```

This example adds a new Prestige 21 difficulty that makes villagers get hungry faster. 

It copies all the modifiers from the previous difficulties and adds a new modifier that makes villagers gain two stacks of the Hunger effect instead of one when they have nothing to eat during a break.




# Embark Rewards

Embark rewards are the rewards you start with when starting a new settlement.

## Good

To add/change embark goods you add it as extra information to an existing `_good.json` file . Example: `coal_good.json`

```json
{
	"guid": "",
	"name": "[Crafting] Coal",
	"embarkGoodMetaRewards": [{
		"name": "Meta Reward Embark Goods Coal",
		"goodAmount": 300,
		"minCost": 30,
		"maxCost": 50
		},{
		"guid": "ExampleMod",
		"name": "New Meta Reward",
		"goodAmount": 1,
		"minCost": 1,
		"maxCost": 1
	}]
}
```

This example edits the existing embark reward from the core game (internally named `Meta Reward Embark Goods Coal`) so it gives you 300 coal but costs between 30 and 50.

The second entry adds a new embark reward that gives you 1 of the new coal and costs 1.

## Effect

To add or change an embark/meta reward effect you will need to create a new file ending with `_metaReward.json`. Example: `Incense_3pm_metaReward.json`

```json
{
	"guid": "",
	"name": "Meta Reward Embark Perk Incense 3pm",
	"type": "EmbarkEffectMetaReward",
	"effect": "Incense_3pm",
	"minCost": 1,
	"maxCost": 1
}
```

This example modifies the 3 Incense per minute effect to cost 1.

NOTE: The "type" must be `EmbarkEffectMetaReward` for it to work.

NOTE: "Effect" is the name of the effect you want to add/change. 
- Go to https://hoodedhorse.com/wiki/Against_the_Storm/Effects for a list of all effects in the game.
- Find the ID of the effect you want from https://github.com/JamesVeug/AgainstTheStormAPI/blob/master/ATS_API/Scripts/Helpers/Enums/EffectTypes.cs
  - At the bottom of the file are the ID of the effect and its display name and description

Example:
1. I want to use [Blightrot_Pruner](https://hoodedhorse.com/wiki/Against_the_Storm/Blightrot_Pruner)
2. So I search the github page for `Blightrot Pruner` and find the below:

`{ EffectTypes.Eggs_For_Cysts, "Eggs For Cysts" }, // Blightrot Pruner - Blightrot spores aren't technically eggs...`

3. Finally, we want to use `Eggs For Cysts` in the .json file.
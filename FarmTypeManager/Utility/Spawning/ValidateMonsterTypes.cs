﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using Newtonsoft.Json.Linq;

namespace FarmTypeManager
{
    public partial class ModEntry : Mod
    {
        /// <summary>Methods used repeatedly by other sections of this mod, e.g. to locate tiles.</summary>
        private static partial class Utility
        {
            /// <summary>Removes any invalid monster types and/or settings from a list.</summary>
            /// <param name="monsterTypes">A list of monster type data.</param>
            /// <param name="areaID">The UniqueAreaID of the related SpawnArea. Required for log messages.</param>
            /// <returns>A new list of MonsterTypes with any invalid monster types and/or settings removed.</returns>
            public static List<MonsterType> ValidateMonsterTypes(List<MonsterType> monsterTypes, string areaID = "")
            {
                if (monsterTypes == null || monsterTypes.Count <= 0) //if the provided list is null or empty
                {
                    return new List<MonsterType>(); //return an empty list
                }

                List<MonsterType> validTypes = Clone(monsterTypes); //create a new copy of the list, to be validated and returned

                for (int x = validTypes.Count - 1; x >= 0; x--) //for each monster type in the new list (iterating backward to allow safe removal)
                {
                    //validate monster names
                    bool validName = false;

                    //TODO: UPDATE THIS LIST after finishing SpawnMonster.cs; it's already out of date and will be moreso if "minelevel" stuff is replaced
                    //NOTE: switch cases copied from SpawnMonster.cs; update this manually if new monsters are added
                    switch (validTypes[x].MonsterName.ToLower()) //avoid any casing issues by making this lower-case
                    {
                        case "bat":
                        case "frostbat":
                        case "frost bat":
                        case "lavabat":
                        case "lava bat":
                        case "iridiumbat":
                        case "iridium bat":
                        case "bigslime":
                        case "big slime":
                        case "biggreenslime":
                        case "big green slime":
                        case "bigblueslime":
                        case "big blue slime":
                        case "bigfrostjelly":
                        case "big frost jelly":
                        case "bigredslime":
                        case "big red slime":
                        case "bigredsludge":
                        case "big red sludge":
                        case "bigpurpleslime":
                        case "big purple slime":
                        case "bigpurplesludge":
                        case "big purple sludge":
                        case "bug":
                        case "armoredbug":
                        case "armored bug":
                        case "duggy":
                        case "dust":
                        case "sprite":
                        case "dustsprite":
                        case "dust sprite":
                        case "spirit":
                        case "dustspirit":
                        case "dust spirit":
                        case "ghost":
                        case "carbonghost":
                        case "carbon ghost":
                        case "slime":
                        case "greenslime":
                        case "green slime":
                        case "blueslime":
                        case "blue slime":
                        case "frostjelly":
                        case "frost jelly":
                        case "redslime":
                        case "red slime":
                        case "redsludge":
                        case "red sludge":
                        case "purpleslime":
                        case "purple slime":
                        case "purplesludge":
                        case "purple sludge":
                        case "grub":
                        case "cavegrub":
                        case "cave grub":
                        case "fly":
                        case "cavefly":
                        case "cave fly":
                        case "mutantgrub":
                        case "mutant grub":
                        case "mutantfly":
                        case "mutant fly":
                        case "metalhead":
                        case "metal head":
                        case "mummy":
                        case "rockcrab":
                        case "rock crab":
                        case "lavacrab":
                        case "lava crab":
                        case "iridiumcrab":
                        case "iridium crab":
                        case "rockgolem":
                        case "rock golem":
                        case "stonegolem":
                        case "stone golem":
                        case "wildernessgolem":
                        case "wilderness golem":
                        case "serpent":
                        case "brute":
                        case "shadowbrute":
                        case "shadow brute":
                        case "shaman":
                        case "shadowshaman":
                        case "shadow shaman":
                        case "skeleton":
                        case "squid":
                        case "kid":
                        case "squidkid":
                        case "squid kid":
                            validName = true; //the name is valid
                            break;
                        default: break; //the name is invalid
                    }

                    if (!validName) //if the name is invalid
                    {
                        Monitor.Log($"A listed monster (\"{validTypes[x].MonsterName}\") doesn't match any known monster types. Make sure that name isn't misspelled in your config file.", LogLevel.Info);
                        Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                        validTypes.RemoveAt(x); //remove this type from the valid list
                        continue; //skip to the next monster type
                    }

                    //validate HP
                    if (validTypes[x].Settings.ContainsKey("HP"))
                    {
                        if (validTypes[x].Settings["HP"] is long) //if this is a readable integer
                        {
                            int HP = Convert.ToInt32(validTypes[x].Settings["HP"]);
                            if (HP < 1) //if the setting is too low
                            {
                                Monitor.Log($"The \"HP\" setting for monster type \"{validTypes[x].MonsterName}\" is {HP}. Setting it to 1.", LogLevel.Trace);
                                validTypes[x].Settings["HP"] = (long)1; //set the validated setting to 1
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"HP\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("HP"); //remove the setting
                        }
                    }

                    //validate damage
                    if (validTypes[x].Settings.ContainsKey("Damage"))
                    {
                        if (validTypes[x].Settings["Damage"] is long) //if this is a readable integer
                        {
                            int damage = Convert.ToInt32(validTypes[x].Settings["Damage"]);
                            if (damage < 0) //if the setting is too low
                            {
                                Monitor.Log($"The \"Damage\" setting for monster type \"{validTypes[x].MonsterName}\" is {damage}. Setting it to 0.", LogLevel.Trace);
                                validTypes[x].Settings["Damage"] = (long)0; //set the validated setting to 0
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"Damage\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("Damage"); //remove the setting
                        }
                    }

                    //validate defense
                    if (validTypes[x].Settings.ContainsKey("Defense"))
                    {
                        if (validTypes[x].Settings["Defense"] is long) //if this is a readable integer
                        {
                            int defense = Convert.ToInt32(validTypes[x].Settings["Defense"]);
                            if (defense < 0) //if the setting is too low
                            {
                                Monitor.Log($"The \"Defense\" setting for monster type \"{validTypes[x].MonsterName}\" is {defense}. Setting it to 0.", LogLevel.Trace);
                                validTypes[x].Settings["Defense"] = (long)0; //set the validated setting to 1
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"Defense\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("Defense"); //remove the setting
                        }
                    }

                    //validate dodge chance
                    if (validTypes[x].Settings.ContainsKey("DodgeChance"))
                    {
                        if (validTypes[x].Settings["DodgeChance"] is long) //if this is a readable integer
                        {
                            int dodge = Convert.ToInt32(validTypes[x].Settings["DodgeChance"]);
                            if (dodge < 1) //if the setting is too low
                            {
                                Monitor.Log($"The \"DodgeChance\" setting for monster type \"{validTypes[x].MonsterName}\" is {dodge}. Setting it to 1.", LogLevel.Trace);
                                validTypes[x].Settings["DodgeChance"] = (long)1; //set the validated setting to 1
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"DodgeChance\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("DodgeChance"); //remove the setting
                        }
                    }

                    //validate movement speed
                    if (validTypes[x].Settings.ContainsKey("Speed"))
                    {
                        if (validTypes[x].Settings["Speed"] is long) //if this is a readable integer
                        {
                            int speed = Convert.ToInt32(validTypes[x].Settings["Speed"]);
                            if (speed < 0) //if the setting is too low
                            {
                                Monitor.Log($"The \"Speed\" setting for monster type \"{validTypes[x].MonsterName}\" is {speed}. Setting it to 0.", LogLevel.Trace);
                                validTypes[x].Settings["Speed"] = (long)0; //set the validated setting to 0
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"Speed\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("Speed"); //remove the setting
                        }
                    }

                    //validate experience points
                    if (validTypes[x].Settings.ContainsKey("EXP"))
                    {
                        if (validTypes[x].Settings["EXP"] is long) //if this is a readable integer
                        {
                            int exp = Convert.ToInt32(validTypes[x].Settings["EXP"]);
                            if (exp < 0) //if the setting is too low
                            {
                                Monitor.Log($"The \"EXP\" setting for monster type \"{validTypes[x].MonsterName}\" is {exp}. Setting it to 0.", LogLevel.Trace);
                                validTypes[x].Settings["EXP"] = (long)0; //set the validated setting to 0
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"EXP\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("EXP"); //remove the setting
                        }
                    }

                    //validate the related skill setting
                    if (validTypes[x].Settings.ContainsKey("RelatedSkill"))
                    {
                        if (validTypes[x].Settings["RelatedSkill"] is string) //if this is a readable string
                        {
                            string relatedSkill = ((string)validTypes[x].Settings["RelatedSkill"]).Trim().ToLower(); //parse the provided skill, trim whitespace, and lower case
                            bool isSkill = false;

                            foreach (string skill in Enum.GetNames(typeof(Skills))) //for each known in-game skill
                            {
                                if (relatedSkill.Trim().ToLower() == skill.Trim().ToLower()) //if the provided skill name matches this known skill
                                {
                                    isSkill = true; //the provided skill is valid
                                }
                            }

                            if (!isSkill) //if this isn't a known skill
                            {
                                Monitor.Log($"The \"RelatedSkill\" setting for monster type \"{validTypes[x].MonsterName}\" doesn't seem to be a known skill. Please make sure it's spelled correctly.", LogLevel.Info);
                                Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                                validTypes[x].Settings.Remove("RelatedSkill"); //remove the setting
                            }
                        }
                        else //if this isn't a readable string
                        {
                            Monitor.Log($"The \"RelatedSkill\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's a valid string (text inside quotation marks).", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("RelatedSkill"); //remove the setting
                        }
                    }

                    //validate skill level requirement
                    if (validTypes[x].Settings.ContainsKey("SkillLevelRequired"))
                    {
                        if (validTypes[x].Settings["SkillLevelRequired"] is long) //if this is a readable integer
                        {
                            if (validTypes[x].Settings.ContainsKey("RelatedSkill")) //if a RelatedSkill has been provided
                            {
                                int required = Convert.ToInt32(validTypes[x].Settings["SkillLevelRequired"]);
                                int highestSkillLevel = 0; //highest skill level among all existing farmers (not just the host)
                                Enum.TryParse((string)validTypes[x].Settings["RelatedSkill"], true, out Skills skill); //parse the RelatedSkill setting into an enum (note: the setting should be validated earlier in this method)

                                foreach (Farmer farmer in Game1.getAllFarmers()) //for each farmer
                                {
                                    
                                    highestSkillLevel = Math.Max(highestSkillLevel, farmer.getEffectiveSkillLevel((int)skill)); //record the farmer's skill level if it's higher than before
                                }

                                if (required > highestSkillLevel) //if the skill requirement is higher than any farmer's skill
                                {
                                    Monitor.Log($"Skipping monster type \"{validTypes[x].MonsterName}\" in spawn area \"{areaID}\" due to skill level requirement.", LogLevel.Trace);
                                    validTypes.RemoveAt(x); //remove this type from the valid list
                                    continue; //skip to the next monster type
                                }
                            }
                            else //if a RelatedSkill was not provided
                            {
                                Monitor.Log($"Monster type \"{validTypes[x].MonsterName}\" has a valid setting for \"SkillLevelRequired\" but not \"RelatedSkill\". The requirement will be skipped.", LogLevel.Info);
                                Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                                validTypes[x].Settings.Remove("SkillLevelRequired"); //remove the setting
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"SkillLevelRequired\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("SkillLevelRequired"); //remove the setting
                        }
                    }

                    //validate HP multiplier
                    if (validTypes[x].Settings.ContainsKey("PercentExtraHPPerSkillLevel"))
                    {
                        if (!(validTypes[x].Settings["PercentExtraHPPerSkillLevel"] is long)) //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"PercentExtraHPPerSkillLevel\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("PercentExtraHPPerSkillLevel"); //remove the setting
                        }

                    }

                    //validate damage multiplier
                    if (validTypes[x].Settings.ContainsKey("PercentExtraDamagePerSkillLevel"))
                    {
                        if (!(validTypes[x].Settings["PercentExtraDamagePerSkillLevel"] is long)) //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"PercentExtraDamagePerSkillLevel\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("PercentExtraDamagePerSkillLevel"); //remove the setting
                        }
                    }

                    //validate speed multiplier
                    if (validTypes[x].Settings.ContainsKey("PercentExtraSpeedPerSkillLevel"))
                    {
                        if (!(validTypes[x].Settings["PercentExtraSpeedPerSkillLevel"] is long)) //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"PercentExtraSpeedPerSkillLevel\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("PercentExtraSpeedPerSkillLevel"); //remove the setting
                        }

                    }

                    //validate experience multiplier
                    if (validTypes[x].Settings.ContainsKey("PercentExtraEXPPerSkillLevel"))
                    {
                        if (!(validTypes[x].Settings["PercentExtraEXPPerSkillLevel"] is long)) //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"PercentExtraEXPPerSkillLevel\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("PercentExtraEXPPerSkillLevel"); //remove the setting
                        }
                    }

                    //validate loot and parse the provided objects into IDs
                    if (validTypes[x].Settings.ContainsKey("Loot"))
                    {
                        List<object> rawList = null;

                        try
                        {
                            rawList = ((JArray)validTypes[x].Settings["Loot"]).ToObject<List<object>>(); //cast this list to catch formatting/coding errors
                        }
                        catch (Exception)
                        {
                            Monitor.Log($"The \"Loot\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's a correctly formatted list.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("Loot"); //remove the setting
                        }

                        if (validTypes[x].Settings.ContainsKey("Loot")) //if no exception happened
                        {
                            if (rawList == null) //if a null list was provided
                            {
                                validTypes[x].Settings["Loot"] = new List<int>(); //use an empty list of IDs
                            }
                            else //if an actual list was provided
                            {
                                validTypes[x].Settings["Loot"] = GetIDsFromObjects(rawList, areaID); //parse the list into valid IDs
                            }
                        }
                    }

                    //validate persistent HP
                    if (validTypes[x].Settings.ContainsKey("PersistentHP"))
                    {
                        if (!(validTypes[x].Settings["PersistentHP"] is bool)) //if this is NOT a readable boolean
                        {
                            Monitor.Log($"The \"PersistentHP\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's true or false (without quotation marks).", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("PersistentHP"); //remove the setting
                        }
                    }

                    //validate current HP
                    if (validTypes[x].Settings.ContainsKey("CurrentHP"))
                    {
                        if (validTypes[x].Settings["CurrentHP"] is long) //if this is a readable integer
                        {
                            int currentHP = Convert.ToInt32(validTypes[x].Settings["CurrentHP"]);
                            if (currentHP < 1) //if the current HP setting is too low
                            {
                                Monitor.Log($"The \"CurrentHP\" setting for monster type \"{validTypes[x].MonsterName}\" is {currentHP}. Setting it to 1.", LogLevel.Trace);
                                monsterTypes[x].Settings["CurrentHP"] = (long)1; //set the original provided setting to 1
                                validTypes[x].Settings["CurrentHP"] = (long)1; //set the validated setting to 1
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"CurrentHP\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("CurrentHP"); //remove the setting
                        }
                    }

                    //validate color
                    if (validTypes[x].Settings.ContainsKey("Color")) //if color was provided
                    {
                        try
                        {
                            string[] colorText = ((string)validTypes[x].Settings["Color"]).Trim().Split(' '); //split the color string into strings for each number
                            int[] colorRGB = new int[] { Convert.ToInt32(colorText[0]), Convert.ToInt32(colorText[1]), Convert.ToInt32(colorText[2]) }; //convert the strings into numbers
                            Color color = new Color(colorRGB[0], colorRGB[1], colorRGB[2]); //set the color variable
                        }
                        catch (Exception)
                        {
                            Monitor.Log($"The \"Color\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it follows the correct format, e.g. \"255 255 255\".", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("Color"); //remove the setting
                        }
                    }
                    else if (validTypes[x].Settings.ContainsKey("MinColor") && validTypes[x].Settings.ContainsKey("MaxColor")) //if color wasn't provided, but mincolor & maxcolor were
                    {
                        try
                        {
                            string[] minColorText = ((string)validTypes[x].Settings["MinColor"]).Trim().Split(' '); //split the setting string into strings for each number
                            int[] minColorRGB = new int[] { Convert.ToInt32(minColorText[0]), Convert.ToInt32(minColorText[1]), Convert.ToInt32(minColorText[2]) }; //convert the strings into numbers

                            string[] maxColorText = ((string)validTypes[x].Settings["MaxColor"]).Trim().Split(' '); //split the setting string into strings for each number
                            int[] maxColorRGB = new int[] { Convert.ToInt32(maxColorText[0]), Convert.ToInt32(maxColorText[1]), Convert.ToInt32(maxColorText[2]) }; //convert the strings into numbers

                            //validate individual color values
                            string validMin = "";
                            string validMax = "";

                            for (int y = 0; y < minColorRGB.Length; y++) //for each color value
                            {
                                if (minColorRGB[y] > maxColorRGB[y]) //if min > max
                                {
                                    //swap min and max
                                    int temp = minColorRGB[y];
                                    minColorRGB[y] = maxColorRGB[y];
                                    maxColorRGB[y] = temp;
                                }

                                //append to new string versions of the settings
                                validMin += minColorRGB[y] + " ";
                                validMax += maxColorRGB[y] + " ";
                            }

                            //update the validated settings
                            validTypes[x].Settings["MinColor"] = validMin.Trim();
                            validTypes[x].Settings["MaxColor"] = validMax.Trim();
                        }
                        catch (Exception)
                        {
                            Monitor.Log($"The \"MinColor\" and/or \"MaxColor\" settings for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure they follow the correct format, e.g. \"255 255 255\".", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            //remove the settings
                            validTypes[x].Settings.Remove("MinColor");
                            validTypes[x].Settings.Remove("MaxColor");
                        }
                    }

                    //validate spawn weight
                    if (validTypes[x].Settings.ContainsKey("SpawnWeight"))
                    {
                        if (validTypes[x].Settings["SpawnWeight"] is long) //if this is a readable integer
                        {
                            int weight = Convert.ToInt32(validTypes[x].Settings["SpawnWeight"]);
                            if (weight < 1) //if the setting is too low
                            {
                                Monitor.Log($"The \"SpawnWeight\" setting for monster type \"{validTypes[x].MonsterName}\" is {weight}. Setting it to 1.", LogLevel.Trace);
                                validTypes[x].Settings["SpawnWeight"] = (long)1; //set to 1
                            }
                        }
                        else //if this isn't a readable integer
                        {
                            Monitor.Log($"The \"SpawnWeight\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's an integer.", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("SpawnWeight"); //remove the setting
                        }
                    }

                    //validate sprite
                    if (validTypes[x].Settings.ContainsKey("Sprite"))
                    {
                        if (validTypes[x].Settings["Sprite"] is string spriteText) //if this is a readable string
                        {
                            try
                            {
                                AnimatedSprite sprite = new AnimatedSprite(spriteText);
                            }
                            catch (Exception)
                            {
                                Monitor.Log($"The \"Sprite\" setting for monster type \"{validTypes[x].MonsterName}\" failed to load. Please make sure the setting is spelled correctly.", LogLevel.Info);
                                Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                                validTypes[x].Settings.Remove("Sprite"); //remove the setting
                            }
                        }
                        else //if this is NOT a readable string
                        {
                            Monitor.Log($"The \"Sprite\" setting for monster type \"{validTypes[x].MonsterName}\" couldn't be parsed. Please make sure it's a valid string (text inside quotation marks).", LogLevel.Info);
                            Monitor.Log($"Affected spawn area: {areaID}", LogLevel.Info);

                            validTypes[x].Settings.Remove("Sprite"); //remove the setting
                        }
                    }
                }

                return validTypes;
            }
        }
    }
}
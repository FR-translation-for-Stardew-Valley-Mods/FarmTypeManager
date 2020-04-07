﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace FarmTypeManager
{
    public partial class ModEntry : Mod
    {
        /// <summary>Tasks performed before the game saves.</summary>
        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            if (Context.IsMainPlayer != true) { return; } //if the player using this mod is a multiplayer farmhand, don't do anything
            if (Utility.DayIsEnding || SaveAnywhereIsSaving) { return; } //if a specialized save process is already handling this, don't do anything

            BeforeMidDaySave();
        }

        /// <summary>Saves and removes custom entities before a mid-day save event.</summary>
        private void BeforeMidDaySave()
        {
            Monitor.Log($"Mid-day save event started. Saving and removing custom objects/data.", LogLevel.Trace);

            Utility.MonsterTracker.Clear(); //clear any tracked monster data (note: this should happen *before* handling monster expiration/removal)

            if (Utility.FarmDataList == null) { return; } //if the farm data list is blank, do nothing

            foreach (FarmData data in Utility.FarmDataList) //for each set of farm data
            {
                if (data.Pack != null) //if this data is from a content pack
                {
                    Monitor.VerboseLog($"Processing save data for content pack: {data.Pack.Manifest.Name}");
                }
                else //this data is from this mod's own folders
                {
                    Monitor.VerboseLog($"Processing save data for FarmTypeManager/data/{Constants.SaveFolderName}_SaveData.save");
                }

                Utility.ProcessObjectExpiration(save: data.Save, endOfDay: false); //remove custom object classes, but do not process expiration settings

                if (data.Pack != null) //if this data is from a content pack
                {
                    data.Pack.WriteJsonFile(Path.Combine("data", $"{Constants.SaveFolderName}_SaveData.save"), data.Save); //update the save file for that content pack
                }
                else //this data is from this mod's own folders
                {
                    Helper.Data.WriteJsonFile(Path.Combine("data", $"{Constants.SaveFolderName}_SaveData.save"), data.Save); //update the save file in this mod's own folders
                }
            }
        }
    }
}

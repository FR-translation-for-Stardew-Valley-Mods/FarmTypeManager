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

namespace FarmTypeManager
{
    public partial class ModEntry : Mod
    {
        /// <summary>Methods used repeatedly by other sections of this mod, e.g. to locate tiles.</summary>
        private static partial class Utility
        {
            /// <summary>Produces a list of x/y coordinates for object spawning at a location (based on tile properties, e.g. the "grass" type).</summary>
            /// <param name="location">The game location to be checked.</param>
            /// <param name="type">A string representing the tile property to match, or a special term used for some additional checks.</param>
            /// <returns>A list of Vector2, each representing a tile for object spawning at the given location.</returns>
            public static List<Vector2> GetTilesByProperty(GameLocation location, string type)
            {
                List<Vector2> tiles = new List<Vector2>(); //will contain x,y coordinates for new object placement

                //get the total size of the current map
                int mapX = location.Map.DisplayWidth / Game1.tileSize;
                int mapY = location.Map.DisplayHeight / Game1.tileSize;

                //the following loops should populate a list of tiles for spawning
                for (int y = 0; y < mapY; y++)
                {
                    for (int x = 0; x < mapX; x++) //loops for each tile on the map, from the top left (x,y == 0,0) to bottom right, moving horizontally first
                    {
                        Vector2 tile = new Vector2(x, y);
                        if (type.Equals("all", StringComparison.OrdinalIgnoreCase)) //if the "property" to be matched is "All" (a special exception)
                        {
                            //add any clear tiles, regardless of properties
                            tiles.Add(tile); //add to list of tiles
                        }
                        else if (type.Equals("diggable", StringComparison.OrdinalIgnoreCase)) //if the tile's "Diggable" property matches (case-insensitive)
                        {
                            if (location.doesTileHaveProperty(x, y, "Diggable", "Back") == "T") //NOTE: the string "T" means "true" for several tile property checks
                            {
                                tiles.Add(tile); //add to list of tiles
                            }
                        }
                        else //assumed to be checking for a specific value in the tile's "Type" property, e.g. "Grass" or "Dirt"
                        {
                            string currentType = location.doesTileHaveProperty(x, y, "Type", "Back") ?? ""; //get the current "Type" property of this tile (or if null, an empty string)

                            if (currentType.Equals(type, StringComparison.OrdinalIgnoreCase)) //if the tile's "Type" property matches (case-insensitive)
                            {
                                tiles.Add(tile); //add to list of tiles
                            }
                        }
                    }
                }
                return tiles;
            }
        }
    }
}
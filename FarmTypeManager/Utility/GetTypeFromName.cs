﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using Newtonsoft.Json;

namespace FarmTypeManager
{
    public partial class ModEntry : Mod
    {
        /// <summary>Methods used repeatedly by other sections of this mod, e.g. to locate tiles.</summary>
        private static partial class Utility
        {
            /// <summary>Searches all loaded assemblies for the first type matching the provided name. Caches to FarmTypeManager.Utility.DynamicTypes for future access.</summary>
            /// <param name="typeName">The full name of the type to be found (e.g. "FarmTypeManager.Monsters.GhostFTM").</param>
            /// <param name="baseClass">If not null, the returned type must be derived from this class.</param>
            /// <returns>Returns the first type found with a matching name, or null if none matched.</returns>
            public static Type GetTypeFromName(string typeName, Type baseClass = null)
            {
                Type matchingType;

                bool filterName(Type type) => type.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase); //true when a type's full name matches the provided name
                bool filterSubclass(Type type) => type.IsSubclassOf(baseClass); //true when a type is derived from the provided base class
                bool filterInvalidAssemblies(Assembly assembly) => //true when an assembly can be checked for the desired type (i.e. the assembly should not cause errors when checked)
                    assembly.IsDynamic == false
                    && assembly.ManifestModule.Name != "<In Memory Module>"
                    && !assembly.FullName.StartsWith("System")
                    && !assembly.FullName.StartsWith("Microsoft");

                if (baseClass != null) //if a base class was provided
                {
                    //if this type already exists in the DynamicTypes list, retrieve it
                    matchingType = DynamicTypes
                        .Where(filterSubclass) //ignore any types that are not subclasses of baseClass
                        .FirstOrDefault(filterName); //get the first assembly with a matching name (or null if no types matched)

                    if (matchingType == null) //if this type isn't in the DynamicTypes list
                    {
                        matchingType =
                        AppDomain.CurrentDomain.GetAssemblies() //get all assemblies
                        .Where(filterInvalidAssemblies) //ignore any assemblies that might cause errors when checked this way
                        .SelectMany(assembly => assembly.GetTypes()) //get all types from each assembly as a single sequence
                        .Where(filterSubclass) //ignore any types that are not subclasses of baseClass
                        .FirstOrDefault(filterName); //get the first assembly with a matching name (or null if no types matched)

                        if (matchingType != null) //if a matching type was found
                        {
                            DynamicTypes.Add(matchingType); //add it to the DynamicTypes list for quicker access
                        }
                    }
                        
                }
                else //if a base class was NOT provided
                {
                    //if this type already exists in the DynamicTypes list, retrieve it
                    matchingType = DynamicTypes
                        .FirstOrDefault(filterName); //get the first assembly with a matching name (or null if no types matched)

                    if (matchingType == null) //if this type isn't in the DynamicTypes list
                    {
                        matchingType =
                        AppDomain.CurrentDomain.GetAssemblies() //get all assemblies
                        .Where(filterInvalidAssemblies) //ignore any assemblies that might cause errors when checked this way
                        .SelectMany(assembly => assembly.GetTypes()) //get all types from each assembly as a single sequence
                        .FirstOrDefault(filterName); //get the first assembly with a matching name (or null if no types matched)

                        if (matchingType != null) //if a matching type was found
                        {
                            DynamicTypes.Add(matchingType); //add it to the DynamicTypes list for quicker access
                        }
                    }   
                }

                return matchingType;
            }
        }
    }
}
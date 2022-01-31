﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MelonLoader.MonoInternals;
using ModHelper;

namespace MelonLoader.CompatibilityLayers
{
    internal class MuseDashModLoader_Module : MelonCompatibilityLayer.Module
    {
        public override void Setup()
        {
            // To-Do:
            // Detect if MuseDashModLoader is already Installed
            // Point AssemblyResolveInfo to already installed MuseDashModLoader Assembly
            // Inject Custom Resolver

            string[] assembly_list =
            {
                "ModHelper",
                "ModLoader",
            };
            Assembly base_assembly = typeof(MuseDashModLoader_Module).Assembly;
            foreach (string assemblyName in assembly_list)
                MonoResolveManager.GetAssemblyResolveInfo(assemblyName).Override = base_assembly;

            MelonBase.CustomMelonResolvers += Resolve;
        }

        private MelonBase[] Resolve(Assembly asm)
        {
            IEnumerable<Type> modTypes = asm.GetValidTypes(x =>
            {
                Type[] interfaces = x.GetInterfaces();
                return (interfaces != null) && interfaces.Any() && interfaces.Contains(typeof(IMod));  // To-Do: Change to Type Reflection based on Setup
            });
            if ((modTypes == null) || !modTypes.Any())
                return null;

            return modTypes.Select(x => LoadMod(asm, x)).ToArray();
        }

        private MelonBase LoadMod(Assembly asm, Type modType)
        {
            var modInstance = Activator.CreateInstance(modType) as IMod;

            var modName = modInstance.Name;

            if (string.IsNullOrEmpty(modName))
                modName = modType.FullName;

            var modVersion = asm.GetName().Version.ToString();
            if (string.IsNullOrEmpty(modVersion) || modVersion.Equals("0.0.0.0"))
                modVersion = "1.0.0.0";

            var melon = MelonBase.CreateWrapper<MuseDashModWrapper>(asm, modName, modVersion);
            melon.modInstance = modInstance;
            ModLoader.ModLoader.mods.Add(modInstance);
            ModLoader.ModLoader.LoadDependency(asm);
            return melon;
        }
    }
}
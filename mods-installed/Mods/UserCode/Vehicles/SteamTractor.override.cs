// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.TechTree
{
    using Eco.Gameplay.Components;
    using Eco.Gameplay.Objects;

    /// <summary>Custom Steam tractor logic goes here, stuff that ins't autogen.</summary>
    [RequireComponent(typeof(VehicleToolComponent))]
    [RequireComponent(typeof(VehicleScoopHandlerComponent))]
    public partial class SteamTractorObject : PhysicsWorldObject
    {
        protected override void PostInitialize()
        {
            var vehicleToolComponent = this.GetComponent<VehicleToolComponent>();
            //setup tractor tool. Scoop module requires tool usage, so it has special logic for it. No other modules use it for now
            vehicleToolComponent.Minable = true;

            var treeHarvesterComponent = this.GetComponent<VehicleTreeHarvestComponent>();
            treeHarvesterComponent.DisableBasicToolSync = true; // on tractor both VehicleToolComponent and VehicleTreeHarvestComponent present, so we need to cut some internal VehicleToolComponent logic from harvester
        }
    }
}

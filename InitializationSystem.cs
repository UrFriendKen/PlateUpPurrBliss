using Kitchen;
using KitchenMods;
using System;
using Unity.Entities;

namespace KitchenPurrBliss
{
    [UpdateBefore(typeof(CreatePets))]
    [UpdateBefore(typeof(PetCatInteraction))]
    internal class InitializationSystem : FranchiseFirstFrameSystem, IModSystem
    {
        protected override void OnUpdate()
        {
            try
            {
                World.GetExistingSystem<CreatePets>().Enabled = false;
            }
            catch (NullReferenceException)
            {
                Main.LogError("Cannot disable CreatePets");
            }
            try
            {
                World.GetExistingSystem<PetCatInteraction>().Enabled = false;
            }
            catch (NullReferenceException)
            {
                Main.LogError("Cannot disable PetCatInteraction");
            }

        }
    }
}

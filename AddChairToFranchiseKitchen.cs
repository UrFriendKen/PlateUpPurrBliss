using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace KitchenPurrBliss
{
    public struct SExtraChairAttempted : IComponentData { }


    [UpdateAfter(typeof(CreateFranchiseKitchen))]
    internal class AddChairToFranchiseKitchen : FranchiseSystem, IModSystem
    {
        internal static int TableSize = -1;

        static float timeout = 10f;
        static float timeRemaining = 0f;

        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (!Has<SExtraChairAttempted>())
            {
                Entity table = GetOccupant(new Vector3(0f, 0f, 2f));
                if (Has<CApplianceTable>(table))
                {
                    Entity chair = Create(GameData.Main.Get<Appliance>(AssetReference.Chair), new Vector3(0f, 0f, 1f), Vector3.up);
                    base.EntityManager.AddComponentData(chair, new CInteractionProxy
                    {
                        Target = table,
                        IsActive = true
                    });

                    if (HasSingleton<SPerformTableUpdate>())
                    {
                        EntityManager.DestroyEntity(GetSingletonEntity<SPerformTableUpdate>());
                    }
                    EntityManager.CreateEntity(typeof(SPerformTableUpdate));
                    Set<SExtraChairAttempted>();
                    Main.LogInfo("Created Chair");
                    TableSize = 3;
                    return;
                }

                if (timeRemaining > timeout)
                {
                    Set<SExtraChairAttempted>();
                    Main.LogInfo("Create Chair Failed!");
                    TableSize = 2;
                    return;
                }
                timeRemaining += Time.DeltaTime;
            }
        }
    }
}

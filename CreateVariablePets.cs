using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace KitchenPurrBliss
{
    public struct SSpawnedCats : IComponentData { }

    internal class CreateVariablePets : FranchiseSystem, IModSystem
    {
        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (Has<SSpawnedCats>())
            {
                return;
            }

            if (Has<SExtraChairAttempted>())
            {
                NewGroup(GetCommandBuffer(ECB.End), AddChairToFranchiseKitchen.TableSize, 1);
                Set<SSpawnedCats>();
            }
        }

        protected void NewGroup(EntityCommandBuffer ecb, int groupSize = 2, int smallCount = 0)
        {
            Entity entity = ecb.CreateEntity(DefaultArchetype);
            ecb.AddComponent<CCustomerGroup>(entity);
            ecb.AddComponent<CPosition>(entity);
            ecb.AddComponent(entity, new CPatience(PatienceReason.Seating));
            ecb.AddComponent(entity, new CCustomerSettings
            {
                BasePatience = PatienceValues.Default,
                Patience = PatienceValues.Default,
                BaseOrdering = OrderingValues.Default,
                Ordering = OrderingValues.Default
            });
            ecb.AddComponent(entity, new CWantsDrink
            {
                Drink = DrinkData.Create(Random.Range(0, 6), Random.Range(0, 6), Random.Range(0, 6))
            });
            ecb.AddComponent<CGroupWait>(entity);
            ecb.AddComponent(entity, new CGroupMealPhase
            {
                Phase = MenuPhase.Starter
            });
            ecb.AddComponent<CGroupReward>(entity);
            ecb.AddBuffer<CGroupMember>(entity);

            int j = 0;
            for (int i = 0; i < groupSize; i++)
            {
                float size = 1f;
                float speed = 1f;
                if (j < smallCount)
                {
                    size = 0.8f;
                    speed = 1.25f;
                    j++;
                }
                ecb.AppendToBuffer(entity, (CGroupMember)NewCustomer(ecb, entity, size, speed));
            }
        }

        protected Entity NewCustomer(EntityCommandBuffer ecb, Entity group, float entityScale = 1f, float entitySpeed = 1f)
        {
            Entity entity = ecb.CreateEntity(DefaultArchetype);
            ecb.AddComponent(entity, new CCustomer
            {
                Scale = entityScale,
                Speed = entitySpeed
            });
            ecb.AddComponent(entity, new CCustomerState
            {
                CurrentState = CCustomerState.State.Normal
            });
            ecb.AddComponent(entity, new CBelongsToGroup
            {
                Group = group
            });
            ecb.AddComponent(entity, default(CCanBePetted));
            ecb.AddComponent(entity, default(CIsInteractive));
            ecb.AddComponent(entity, new CPosition(new Vector3(Random.Range(-2, 5), 0f, 1f)));
            ecb.AddComponent(entity, new CRequiresView
            {
                Type = ViewType.CustomerCat,
                PhysicsDriven = true
            });
            return entity;
        }

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
        }
    }
}

using Kitchen;
using Kitchen.Components;
using KitchenMods;
using MessagePack;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenPurrBliss.Views
{
    public class VisualEffectWhilePettingView : UpdatableObjectView<VisualEffectWhilePettingView.ViewData>
    {
        public GameObject VfxGameObject;

        public SoundSource SoundSource;

        public List<AudioClip> AudioClips;

        public class UpdateView : IncrementalViewSystemBase<VisualEffectWhilePettingView.ViewData>, IModSystem
        {
            private EntityQuery Views;

            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(new QueryHelper()
                    .All(typeof(CCanBePetted)));
            }

            protected override void OnUpdate()
            {
                using var entities = Views.ToEntityArray(Allocator.Temp);
                using var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                for (var i = 0; i < views.Length; i++)
                {
                    var view = views[i];

                    bool active = false;
                    bool playSound = false;
                    if (Require(entities[i], out CIsBeingPetted beingPetted))
                    {
                        if (beingPetted.TimeRemaining < 0f)
                        {
                            EntityManager.RemoveComponent<CIsBeingPetted>(entities[i]);
                        }
                        else
                        {
                            if (beingPetted.TimeSinceLastSound >= beingPetted.SoundTimeInterval)
                            {
                                beingPetted.TimeSinceLastSound = 0f;
                                playSound = true;
                            }
                            beingPetted.TimeRemaining -= Time.DeltaTime;
                            Set(entities[i], beingPetted);
                            active = true;
                        }
                    }

                    ViewData data = new ViewData
                    {
                        Active = active,
                        PlaySound = playSound,
                        Pitch = beingPetted.PitchMultiplier,
                        Volume = beingPetted.VolumeMultiplier
                    };

                    SendUpdate(view, data);
                }
            }
        }

        // you must mark your ViewData as MessagePackObject and mark each field with a key
        // if you don't, the game will run locally but fail in multiplayer
        [MessagePackObject]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public bool Active;
            [Key(2)] public bool PlaySound;
            [Key(3)] public float Volume;
            [Key(4)] public float Pitch;

            // this tells the game how to find this subview within a prefab
            // GetSubView<T> is a cached method that looks for the requested T in the view and its children
            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<VisualEffectWhilePettingView>();

            // this is used to determine if the data needs to be sent again
            public bool IsChangedFrom(ViewData check) => Active != check.Active || PlaySound != check.PlaySound;
        }

        // this receives the updated data from the ECS backend whenever a new update is sent
        // in general, this should update the state of the view to match the values in view_data
        // ideally ignoring all current state; it's possible that not all updates will be received so
        // you should avoid relying on previous state where possible
        protected override void UpdateData(ViewData view_data)
        {
            // perform the update here
            // this is a Unity MonoBehavior so we can do normal Unity things here

            Main.LogInfo("UpdateData");

            if (view_data.PlaySound && AudioClips.Count > 0)
            {
                SoundSource.VolumeMultiplier = view_data.Volume;
                SoundSource.Pitch = view_data.Pitch;
                SoundSource.Configure(SoundCategory.Effects, AudioClips[Mathf.RoundToInt(Random.Range(0, AudioClips.Count))]);
                SoundSource.Play();
            }

            VfxGameObject.SetActive(view_data.Active);
        }
    }
}
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
        // Public fields that are assigned when instantiating view component in GDO
        // To be assigned in OnRegister() of CustomGDO
        public GameObject VfxGameObject;
        public SoundSource SoundSource;
        public List<AudioClip> AudioClips;




        #region ECS View System (Runs on host and updates views to be broadcasted to clients)
        // Nesting the ViewSystemBase within the View class like this is not a requirement, but helps simplify referencing ViewData and keeps everything organised within the view.
        // There are multiple types that inherit ViewSystemBase. Use whichever is appropriate for your application.
        /*
         * IncrementalViewSystemBase<T>: Only sends update when there are changes to ViewData to be sent.
         * GameViewSystemBase<T>: Inherits IncrementalViewSystemBase<T>. Provides `bool HasStatus(RestaurantStatus status)`, but is otherwise identical to IncrementalViewSystemBase<T>.
         * ResponsiveViewSystemBase<TView, TResp>: Use this when you require a response from client to host.
         */
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Views;

            protected override void Initialise()
            {
                base.Initialise();

                // Cache Entity Queries
                // This should contain all IComponentData that will be used in the class
                Views = GetEntityQuery(new QueryHelper()
                    .All(typeof(CCanBePetted), typeof(CLinkedView)));
            }

            protected override void OnUpdate()
            {
                // Do regular ECS stuff here.

                using var entities = Views.ToEntityArray(Allocator.Temp);
                using var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                // Each entity has a CLinkedView that must be updated individually.
                for (var i = 0; i < views.Length; i++)
                {
                    // Main view specific to the entity
                    var view = views[i];

                    // Prepare View Data to be sent
                    bool active = false;
                    if (Require(entities[i], out CIsBeingPetted beingPetted))
                    {
                        if (beingPetted.TimeRemaining < 0f)
                        {
                            EntityManager.RemoveComponent<CIsBeingPetted>(entities[i]);
                        }
                        else
                        {
                            beingPetted.TimeRemaining -= Time.DeltaTime;
                            Set(entities[i], beingPetted);
                            active = true;
                        }
                    }
                    ViewData data = new ViewData
                    {
                        Active = active,
                        Pitch = beingPetted.PitchMultiplier,
                        Volume = beingPetted.VolumeMultiplier,
                        SoundInterval = beingPetted.SoundTimeInterval
                    };

                    // You don't have to perform optimizations here.
                    // SendUpdate() will only broadcast to all players if there are changes to the data. (See ViewData definition below)
                    SendUpdate(view, data);
                }
            }
        }
        #endregion




        #region Message Packet
        // Definition of Message Packet that will be broadcasted to clients
        // This should contain the minimum amount of data necessary to perform the view's function.
        // You MUST mark your ViewData as MessagePackObject
        // If you don't, the game will run locally but fail in multiplayer
        [MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            // You MUST also and mark each field with a key
            // All players must be running versions of the game with the same assigned keys.
            // It is recommended not to change keys after releasing your mod
            // The specifc key used does not matter, as long as there is no overlap.
            [Key(1)] public bool Active;
            [Key(2)] public float Volume;
            [Key(3)] public float Pitch;
            [Key(4)] public float SoundInterval;


            /// <summary>
            /// Find cached subview instance within a prefab from its main view
            /// </summary>
            /// <param name="view">Main view (eg. ApplianceView/ItemView/etc.)</param>
            /// <returns>Subview instance of type T</returns>
            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<VisualEffectWhilePettingView>();    // This is very standardized. Just replace T with your view type


            /// <summary>
            /// Check if data has changed since last update. This is called by view system to determine if an update should be sent
            /// </summary>
            /// <param name="cached">Cached state from last update</param>
            /// <returns>Returns true if data has changed, false otherwise</returns>
            public bool IsChangedFrom(ViewData cached)
            {
                return Active != cached.Active; // Only have to check for inequality between fields that should trigger an update.
            }
        }
        #endregion




        // Non-public fields (Current state of view)
        protected bool _playSound;
        protected float _volumeMultiplier;
        protected float _pitch;
        protected float _soundInterval;
        protected float _soundDelayProgress = 0f;

        // This receives the updated data from the ECS backend whenever an update is sent
        // In general, this should update the state of the view to match the values in view_data
        // ideally ignoring all current state; it's possible that not all updates will be received so
        // you should avoid relying on previous state (Non-public fields above) where possible
        protected override void UpdateData(ViewData view_data)
        {
            // Perform the update here
            // This is a Unity MonoBehavior so we can do normal Unity things here
            

            // Update fields
            _playSound = view_data.Active;
            _volumeMultiplier = view_data.Volume;
            _pitch = view_data.Pitch;
            _soundInterval = view_data.SoundInterval;
            _soundDelayProgress = view_data.SoundInterval;


            // You can also perform actions that require a single update whenever the the view is updated here.
            VfxGameObject.SetActive(view_data.Active);
        }



        // This runs locally for each player every frame
        void Update()
        {
            // Remember that this is Monobehaviour, not ECS

            if (_playSound && AudioClips.Count > 0)
            {
                if (_soundDelayProgress > _soundInterval)
                {
                    SoundSource.VolumeMultiplier = _volumeMultiplier;
                    SoundSource.Pitch = _pitch;
                    SoundSource.Configure(SoundCategory.Effects, AudioClips[Mathf.RoundToInt(Random.Range(0, AudioClips.Count))]);
                    SoundSource.Play();

                    _soundDelayProgress = 0f;
                }
                _soundDelayProgress += Time.deltaTime;
            }
            else
            {
                _soundDelayProgress = 0f;
            }
        }
    }
}
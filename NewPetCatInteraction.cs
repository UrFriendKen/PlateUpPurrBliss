using Kitchen;
using Unity.Entities;

namespace KitchenPurrBliss
{
    public struct CIsBeingPetted : IComponentData
    {
        public float TimeRemaining;
        public float TimeSinceLastSound;
        public float SoundTimeInterval;
        public float PitchMultiplier;
        public float VolumeMultiplier;
    }

    [UpdateInGroup(typeof(LowPriorityInteractionGroup))]
    public class NewPetCatInteraction : ItemInteractionSystem
    {
        const float VFX_LINGER_TIME = 2.5f;
        const float SOUND_INTERVAL = 2.5f;

        protected override bool RequireHold => true;

        protected override bool RequirePress => false;

        protected override bool IsPossible(ref InteractionData data)
        {
            if (!Has<CCanBePetted>(data.Target))
            {
                return false;
            }
            return true;
        }

        protected override void Perform(ref InteractionData data)
        {
            if (!Require(data.Target, out CIsBeingPetted beingPetted))
            {
                float pitchMultiplier = 1f;
                float volumeMultiplier = 0.5f;
                if (Require(data.Target, out CCustomer customer))
                {
                    pitchMultiplier /= customer.Scale;
                }

                beingPetted = new CIsBeingPetted
                {
                    TimeRemaining = VFX_LINGER_TIME,
                    TimeSinceLastSound = SOUND_INTERVAL,
                    SoundTimeInterval = SOUND_INTERVAL,
                    PitchMultiplier = pitchMultiplier,
                    VolumeMultiplier = volumeMultiplier
                };
            }
            beingPetted.TimeSinceLastSound += Time.DeltaTime;
            beingPetted.TimeRemaining = VFX_LINGER_TIME;
            Set(data.Target, beingPetted);
        }
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ReinforcedMechanoids
{
    public class CompProperties_PlaySoundInInterval : CompProperties
    {
        public SoundDef sound;
        public IntRange intervalRange;
        public CompProperties_PlaySoundInInterval()
        {
            this.compClass = typeof(CompPlaySoundInInterval);
        }
    }

    public class CompPlaySoundInInterval : ThingComp
    {
        public CompProperties_PlaySoundInInterval Props => base.props as CompProperties_PlaySoundInInterval;
        private int ticksToNextCall = -1;
        public override void CompTick()
        {
            base.CompTick();
            if (ticksToNextCall < 0)
            {
                ticksToNextCall = Props.intervalRange.RandomInRange;
            }
            ticksToNextCall--;
            if (ticksToNextCall <= 0)
            {
                if (this.parent.Spawned && Find.CameraDriver.CurrentViewRect.ExpandedBy(10).Contains(parent.Position) 
                    && !parent.Position.Fogged(parent.Map))
                {
                    SoundInfo info = SoundInfo.InMap(new TargetInfo(this.parent.Position, this.parent.Map));
                    Props.sound.PlayOneShot(info);
                }
                ticksToNextCall = Props.intervalRange.RandomInRange;
            }
        }
    }
}

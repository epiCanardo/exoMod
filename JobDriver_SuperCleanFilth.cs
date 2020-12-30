// Decompiled with JetBrains decompiler
// Type: RimWorld.JobDriver_CleanFilth
// Assembly: Assembly-CSharp, Version=1.2.7558.21380, Culture=neutral, PublicKeyToken=null
// MVID: D72310B4-D8F6-4D25-AEE5-02792B58549F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin64_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
    public class JobDriver_SuperCleanFilth : JobDriver
    {
        private float cleaningWorkDone;
        private float totalCleaningWorkDone;
        private float totalCleaningWorkRequired;
        private const TargetIndex FilthInd = TargetIndex.A;

        private Filth Filth => (Filth)this.job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil initExtractTargetFromQueue = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A);
            yield return initExtractTargetFromQueue;
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(TargetIndex.A);
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue).JumpIfOutsideHomeArea(TargetIndex.A, initExtractTargetFromQueue);
            Toil clean = new Toil()
            {
                initAction = (Action)(() =>
               {
                   this.cleaningWorkDone = 0.0f;
                   this.totalCleaningWorkDone = 0.0f;
                   this.totalCleaningWorkRequired = this.Filth.def.filth.cleaningWorkToReduceThickness * (float)this.Filth.thickness;
               })
            };
            clean.tickAction = (Action)(() =>
           {
               Filth filth = this.Filth;
               cleaningWorkDone = this.cleaningWorkDone + 10;
               totalCleaningWorkDone = this.totalCleaningWorkDone + 10;
               pawn.needs.rest.CurLevel -= 10;
               if ((double)this.cleaningWorkDone <= (double)filth.def.filth.cleaningWorkToReduceThickness)
                   return;
               filth.ThinFilth();
               this.cleaningWorkDone = 0.0f;
               if (!filth.Destroyed)
                   return;
               clean.actor.records.Increment(RecordDefOf.MessesCleaned);
               this.ReadyForNextToil();
           });
            clean.defaultCompleteMode = ToilCompleteMode.Never;
            clean.WithEffect(EffecterDefOf.Clean, TargetIndex.A);
            clean.WithProgressBar(TargetIndex.A, (Func<float>)(() => this.totalCleaningWorkDone / this.totalCleaningWorkRequired), true);
            clean.PlaySustainerOrSound((Func<SoundDef>)(() =>
           {
               ThingDef def = this.Filth.def;
               return !def.filth.cleaningSound.NullOrUndefined() ? def.filth.cleaningSound : SoundDefOf.Interact_CleanFilth;
           }));
            clean.JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, initExtractTargetFromQueue);
            clean.JumpIfOutsideHomeArea(TargetIndex.A, initExtractTargetFromQueue);
            yield return clean;
            yield return Toils_Jump.Jump(initExtractTargetFromQueue);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.cleaningWorkDone, "cleaningWorkDone");
            Scribe_Values.Look<float>(ref this.totalCleaningWorkDone, "totalCleaningWorkDone");
            Scribe_Values.Look<float>(ref this.totalCleaningWorkRequired, "totalCleaningWorkRequired");
        }
    }
}

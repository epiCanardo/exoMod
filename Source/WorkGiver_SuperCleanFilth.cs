using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
    internal class WorkGiver_SuperCleanFilth : WorkGiver_Scanner
    {
        private int MinTicksSinceThickened = 600;

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Filth);

        public override int MaxRegionsToScanBeforeGlobalSearch => 4;

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => (IEnumerable<Thing>)pawn.Map.listerFilthInHomeArea.FilthInHomeArea;

        public override bool ShouldSkip(Pawn pawn, bool forced = false) => pawn.Map.listerFilthInHomeArea.FilthInHomeArea.Count == 0;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) => t is Filth filth && filth.Map.areaManager.Home[filth.Position] && (pawn.CanReserve((LocalTargetInfo)t, ignoreOtherReservations: forced) && filth.TicksSinceThickened >= this.MinTicksSinceThickened);

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {           
            Job job = JobMaker.MakeJob(JobDefOfExt.SuperClean);
            job.AddQueuedTarget(TargetIndex.A, (LocalTargetInfo)t);
            int num = 15;
            Map map = t.Map;
            Room room = t.GetRoom();
            for (int index1 = 0; index1 < 100; ++index1)
            {
                IntVec3 intVec3 = t.Position + GenRadial.RadialPattern[index1];
                if (intVec3.InBounds(map) && intVec3.GetRoom(map) == room)
                {
                    List<Thing> thingList = intVec3.GetThingList(map);
                    for (int index2 = 0; index2 < thingList.Count; ++index2)
                    {
                        Thing t1 = thingList[index2];
                        if (this.HasJobOnThing(pawn, t1, forced) && t1 != t)
                            job.AddQueuedTarget(TargetIndex.A, (LocalTargetInfo)t1);
                    }
                    if (job.GetTargetQueue(TargetIndex.A).Count >= num)
                        break;
                }
            }
            if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
            {
                job.targetQueueA.SortBy<LocalTargetInfo, int>((Func<LocalTargetInfo, int>)(targ => targ.Cell.DistanceToSquared(pawn.Position)));
            }
            return job;
        }
    }
}

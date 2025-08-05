using System;
using Verse;

namespace PrimarchAssault.AssaultEvent
{
    public abstract class AssaultEventActionProperties
    {
        public string actionName = "All";
        public string eventNotificationText;

        public FleckDef fleckOnChampion;
        public SoundDef sound;
        
        public abstract Type AssaultEventClass();
    }
}
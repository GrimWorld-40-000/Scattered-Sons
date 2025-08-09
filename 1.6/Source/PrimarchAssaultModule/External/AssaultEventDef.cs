using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RimWorld;
using PrimarchAssault.AssaultEvent;
using Verse;

namespace PrimarchAssault.External
{
    public class AssaultEventDef: Def
    {
        public List<AssaultEventAction> Actions;
        public List<AssaultEventActionProperties> actionProperties;
        //Whether to trigger all actions, or select a random one
        public bool triggerAllActions = true;
        
        
        
        public override void PostLoad()
        {
            base.PostLoad();
            InitializeComps();
        }


        private void InitializeComps()
        {
            Actions = new List<AssaultEventAction>();
            if (actionProperties.NullOrEmpty()) return;
            foreach (var t in actionProperties)
            {
                AssaultEventAction action = null;
                try
                {
                    action = (AssaultEventAction) Activator.CreateInstance(t.AssaultEventClass());
                    action.parent = this;
                    Actions.Add(action);
                    action.Initialize(t);
                }
                catch (Exception ex)
                {
                    Log.Error("Could not instantiate or initialize an assault action: " + ex);
                    Actions.Remove(action);
                }
            }
        }

        public void FireAllComps(Map map)
        {
            if (triggerAllActions)
            {
                foreach (AssaultEventAction assaultEventAction in Actions.Where(assaultEventAction => assaultEventAction.Enabled))
                {
                    assaultEventAction.Apply(map);
                }
            }
            else
            {
                List<AssaultEventAction> validActions =
                    Actions.Where(assaultEventAction => assaultEventAction.Enabled).ToList();

                if (validActions.Any())
                {
                    Actions.RandomElement().Apply(map);
                }
            }
        }
    }
}
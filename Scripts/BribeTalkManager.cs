using UnityEngine;

namespace DaggerfallWorkshop.Game
{
    // TalkManager is implemented as a partial which means we can
    // do this to get access to private fields and methods.
    public partial class TalkManager : MonoBehaviour
    {
        bool IsTalkingToStaticNPC => StaticNPC != null;
        bool IsTalkingToMobileNPC => MobileNPC != null;

        public string BribeNPC(ListItem currentTopic)
        {
            var bribeSystem = new BribeSystem(GameManager.Instance.PlayerEntity);
            string answer = string.Empty;

            if (IsTalkingToStaticNPC)
            {
                var npc = this.GetStaticNPCData();
                if (npc.IsNoble())
                {
                    answer = NobleResponses.GetRandomRejection();
                }
                else if (npc.IsSwornToSacredorder())
                {
                    answer = "Are you trying to insult me?";
                }
            }
            else if (IsTalkingToMobileNPC)
            {
                // good faction data to check for flavor
                // bool isGuard = talkManager.MobileNPC.IsGuard;
                // var npc = talkManager.MobileNPC.GetNPCData();
                if (DoesNPCKnowAboutItem(currentTopic))
                {
                    // add entries into the tokens(??) in the subrecords(??)
                    // so that i can expand custom macros when bribes are rejected.
                    answer = bribeSystem.TakeBribe()
                        ? GetKeySubjectBuildingOnMap()
                        : "You, ah...seem to be a few Septims short...";
                }
                else // they don't know
                {
                    answer = GetAnswerWhereIs(currentTopic);
                }
            }

            return answer;
        }

        private bool DoesNPCKnowAboutItem(ListItem currentTopic)
        {
            var knowledge = GetNPCKnowledgeAboutItem(currentTopic);
            return knowledge == NPCKnowledgeAboutItem.KnowsAboutItem;
        }
    }
}
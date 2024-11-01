using UnityEngine;

namespace DaggerfallWorkshop.Game
{
    // TalkManager is implemented as a partial which means we can
    // do this to get access to private fields and methods.
    public partial class TalkManager : MonoBehaviour
    {
        bool IsTalkingToStaticNPC => StaticNPC != null;
        bool IsTalkingToMobileNPC => MobileNPC != null;
        float guardBribeDifficulty = 1.2f;

        public string BribeNPC(ListItem currentTopic, BribeSystem bribeSystem)
        {
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
                var personalityMod = GameManager.Instance.PlayerEntity.Stats.LivePersonality * 0.1f;
                if (MobileNPC.IsGuard)
                {
                    var roll = Random.Range(0.0f, 1.5f) + personalityMod;
                    if (roll >= guardBribeDifficulty)
                    {
                        answer = GetBribeAnswer(bribeSystem);
                    }
                    else
                    {
                        bribeSystem.TakeBribe();
                        answer = "Out of here before I box your ears and haul you in, welp!";
                    }
                }
                else if (DoesNPCKnowAboutItem(currentTopic))
                {
                    var npc = MobileNPC.GetNPCData();
                    // see if i add entries into the tokens(??) in the subrecords(??)
                    // so that i can expand custom macros when bribes are rejected.
                    answer = GetBribeAnswer(bribeSystem);
                }
                else // they don't know
                {
                    answer = GetAnswerWhereIs(currentTopic);
                }
            }

            return answer;
        }

        private string GetBribeAnswer(BribeSystem bribeSystem)
        {
            return bribeSystem.TakeBribe()
                ? GetKeySubjectBuildingOnMap()
                : "You, ah...seem to be a few Septims short...";
        }

        private bool DoesNPCKnowAboutItem(ListItem currentTopic)
        {
            var knowledge = GetNPCKnowledgeAboutItem(currentTopic);
            return knowledge == NPCKnowledgeAboutItem.KnowsAboutItem;
        }
    }
}
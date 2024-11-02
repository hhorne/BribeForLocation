using UnityEngine;
using DaggerfallWorkshop.Game.Entity;
using BribeForLocation;
using DaggerfallWorkshop.Game;

public class BribeSystem
{
    readonly PlayerEntity player;
    float guardBribeCriticalFail = 0.1f;
    float guardBribeDifficulty = 1.2f;
    float personalityBonus => GameManager.Instance.PlayerEntity.Stats.LivePersonality * 0.01f;

    float personalityScale
    {
        get
        {
            var personality = GameManager.Instance.PlayerEntity.Stats.LivePersonality;

            // Linear scaling from 2.0f to 0.0f
            return 2.0f - (personality - 1) * (2.0f / 99.0f);
        }
    }

    public BribeSystem(PlayerEntity player)
    {
        this.player = player;
    }

    public bool CanBribe(TalkManager.ListItem topic) => GetBribeAmount(topic) < player.GoldPieces;

    public string GetBribeResponse(BribeableNPCData npc, TalkManager.ListItem topic)
    {
        string answer;
        if (npc.IsNoble)
        {
            answer = NobleResponses.GetRandomRejection();
        }
        else if (npc.IsSwornToSacredorder)
        {
            answer = "Are you trying to insult me?";
        }
        else if (npc.IsSpyMaster)
        {
            answer = "Is this an attempt at mockery?";
        }
        else if (npc.IsGuard)
        {
            answer = AttemptBribeOnGuard(topic);
        }
        else if (BribeForLocationMain.Settings.EnableKnowlegeChecking)
        {
            // see if i add entries into the tokens(??) in the subrecords(??)
            // so that i can expand custom macros when bribes are rejected.
            var result = ExperimentalFeatures.GetNPCKnowledgeAboutItem(topic);
            switch (result)
            {
                case TalkManager.NPCKnowledgeAboutItem.DoesNotKnowAboutItem:
                    answer = TalkManager.Instance.GetAnswerWhereIs(topic);
                    break;
                case TalkManager.NPCKnowledgeAboutItem.KnowsAboutItem:
                default:
                    answer = GetMarkMapResponse(topic);
                    break;
            }
        }
        else
        {
            answer = GetMarkMapResponse(topic);
        }

        return answer;
    }

    public int GetBribeAmount(TalkManager.ListItem topic)
    {
        // - Location
        // - People (assuming quest, maybe there's a premium to this kind of info)
        // - Work (Not As Cheap, Randomly doesn't yield a "good" response.)
        var s = BribeForLocationMain.Settings;
        int bribeAmount = s.StartingBribeAmount;

        switch (topic.questionType)
        {
            case TalkManager.QuestionType.Work:
                bribeAmount += s.WorkFee;
                break;
            case TalkManager.QuestionType.Person:
                bribeAmount += s.PeopleFee;
                break;
            default:
                break;
        }

        if (s.ScaleByLevel)
        {
            float leveledMultiplier = 1 + (player.Level * s.AmountToScaleBy);
            bribeAmount = Mathf.RoundToInt(bribeAmount * leveledMultiplier);
        }

        bribeAmount = Mathf.RoundToInt(bribeAmount * personalityScale);

        return bribeAmount;
    }

    public bool TakeBribe(TalkManager.ListItem topic)
    {
        if (CanBribe(topic))
        {
            player.GoldPieces -= GetBribeAmount(topic);
            return true;
        }

        return false;
    }

    string AttemptBribeOnGuard(TalkManager.ListItem topic)
    {
        string answer;
        var roll = Random.Range(0.0f, 1.5f) + personalityBonus;
        if (roll >= guardBribeDifficulty)
        {
            answer = GetMarkMapResponse(topic);
        }
        else if (roll <= guardBribeCriticalFail)
        {
            TakeBribe(topic);
            answer = "Out of here before I box your ears and haul you in, welp!";
        }
        else
        {
            answer = "I know when I'm being setup.";
        }
        return answer;
    }

    string GetMarkMapResponse(TalkManager.ListItem topic)
    {
        TakeBribe(topic);
        return TalkManager.Instance.GetKeySubjectBuildingOnMap();
    }
}

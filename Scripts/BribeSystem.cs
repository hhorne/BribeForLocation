using UnityEngine;
using DaggerfallWorkshop.Game.Entity;
using BribeForLocation;
using DaggerfallWorkshop.Game;

public class BribeSystem
{
    // not the REAL the attributeMax but right for our scaling formula
    const int attributeMax = 100;
    const int attributeMin = 1;

    // don't think I want this in settings, maybe build a dictionary of regions
    // and have that influence how easy/hard it is.
    float guardBribeCriticalFail = 0.1f;
    float guardBribeDifficulty = 1.2f;

    PlayerEntity player => GameManager.Instance.PlayerEntity;
    int personality => player.Stats.LivePersonality;
    float personalityBonus => player.Stats.LivePersonality * 0.01f;

    public BribeSystem()
    {
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
                default: // Default handles the case when the Feature doesn't work
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
        var s = BribeForLocationMain.Settings;
        int bribeAmount = s.StartingBribeAmount;

        // add premiums before scaling
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

        if (s.EnableScaleByLevel)
        {
            float scaledMultiplier = 1 + (player.Level * s.AmountToScaleBy);
            bribeAmount = Mathf.RoundToInt(bribeAmount * scaledMultiplier);
        }

        if (s.ScaleByPersonality)
        {
            float scaledMultiplier = GetPersonalityScaler();
            bribeAmount = Mathf.RoundToInt(bribeAmount * scaledMultiplier);
        }

        return bribeAmount;
    }

    // Inverse Linear scaling by Personality ex: from 0f, to 2f
    // Personality 100 = scale 0f, i.e. it multiplies bribes to be free.
    // Personality 1 = scale 2, i.e. it doubles the cost of a bribe.
    public float GetPersonalityScaler()
    {
        var s = BribeForLocationMain.Settings;
        float from = s.PersonalityScaleMin;
        float to = s.PersonalityScaleMax;
        float scaledValue = (to - from) * ((personality - attributeMin) / (attributeMax - attributeMin)) + from;
        return scaledValue;
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

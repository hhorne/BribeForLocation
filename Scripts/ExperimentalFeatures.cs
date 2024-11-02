using System;
using System.Reflection;
using DaggerfallWorkshop.Game;
using UnityEngine;

public static class ExperimentalFeatures
{
    private static BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
    public static TalkManager.NPCKnowledgeAboutItem GetNPCKnowledgeAboutItem(TalkManager.ListItem topic)
    {
        TalkManager.NPCKnowledgeAboutItem npcKnowledge;
        try
        {
            npcKnowledge = (TalkManager.NPCKnowledgeAboutItem)typeof(TalkManager)
                .GetMethod(nameof(GetNPCKnowledgeAboutItem), bindingFlags)
                .Invoke(TalkManager.Instance, new object[] { topic });
        }
        catch (Exception e)
        {
            npcKnowledge = TalkManager.NPCKnowledgeAboutItem.NotSet;
            Debug.Log($"Exception in {nameof(ExperimentalFeatures)}.{nameof(GetNPCKnowledgeAboutItem)}: {e.Message}");
        }
        return npcKnowledge;
    }
}

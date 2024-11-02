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
                .Invoke(TalkManager.Instance, new[] { topic });
        }
        catch (Exception e)
        {
            // NotSet is how we'll indicate to the consuming BribeSystem that this didn't
            // really work out and it can't use this.
            npcKnowledge = TalkManager.NPCKnowledgeAboutItem.NotSet;
            Debug.Log($"Exception in {nameof(ExperimentalFeatures)}.{nameof(GetNPCKnowledgeAboutItem)}: {e.Message}");
        }
        return npcKnowledge;
    }
}

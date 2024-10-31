using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BribeForLocation;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Player;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop;

public static class BribeHelperExtensions
{
    /// <summary>
    /// Sworn Orders where members are likely to have their honor insulted
    /// by being offered a bribe.
    /// </summary>
    public static readonly FactionFile.GuildGroups[] SacredOrders = new[]
    {
        FactionFile.GuildGroups.KnightlyOrder,
        FactionFile.GuildGroups.HolyOrder,
    };

    public static bool IsNoble(this TalkManager.NPCData npc) => npc.socialGroup == FactionFile.SocialGroups.Nobility;

    public static bool IsSwornToSacredorder(this TalkManager.NPCData npc) => SacredOrders.Contains(npc.guildGroup);

    public static bool IsItem(this TalkManager.ListItem topic) => topic.type == TalkManager.ListItemType.Item;

    /// <summary>
    /// Combination of code from:
    ///   - TalkManager.GetStaticNPCFactionData
    ///   - TalkManager.SetTargetNPC
    /// Get NPCData for the currently selected StaticNPC.
    /// </summary>
    /// <param name="factionId">The NPC faction ID.</param>
    /// <param name="buildingType">The NPC location building type.</param>
    public static TalkManager.NPCData GetStaticNPCData(this TalkManager talkManager)
    {
        var factionId = talkManager.StaticNPC.Data.factionID;
        var buildingType = GameManager.Instance.PlayerEnterExit.BuildingType;

        if (factionId == 0)
        {
            // Matched to classic: an NPC with a null faction id is assigned to court or people of current region
            if (buildingType == DFLocation.BuildingTypes.Palace)
                factionId = GameManager.Instance.PlayerGPS.GetCourtOfCurrentRegion();
            else
                factionId = GameManager.Instance.PlayerGPS.GetPeopleOfCurrentRegion();
        }
        else if (factionId == (int)FactionFile.FactionIDs.Random_Ruler ||
                 factionId == (int)FactionFile.FactionIDs.Random_Noble ||
                 factionId == (int)FactionFile.FactionIDs.Random_Knight)
        {
            // Change from classic: use "Court of" current region for Random Ruler, Random Noble
            // and Random Knight because these generic factions have no use at all
            factionId = GameManager.Instance.PlayerGPS.GetCourtOfCurrentRegion();
        }

        FactionFile.FactionData factionData;
        GameManager.Instance.PlayerEntity.FactionData.GetFactionData(factionId, out factionData);

        var npcData = new TalkManager.NPCData
        {
            socialGroup = factionData.sgroup < 5
                ? (FactionFile.SocialGroups)factionData.sgroup
                : FactionFile.SocialGroups.Merchants,
            guildGroup = (FactionFile.GuildGroups)factionData.ggroup,
            factionData = factionData,
            race = TalkManager.Instance.StaticNPC.Data.race,
            isSpyMaster = false
        };

        return npcData;
    }

    public static TalkManager.NPCData GetNPCData(this MobilePersonNPC npc)
    {
        FactionFile.FactionData factionData;
        int npcFactionId = GameManager.Instance.PlayerGPS.GetPeopleOfCurrentRegion();
        GameManager.Instance.PlayerEntity.FactionData.GetFactionData(npcFactionId, out factionData);

        return new TalkManager.NPCData
        {
            socialGroup = FactionFile.SocialGroups.Commoners,
            guildGroup = FactionFile.GuildGroups.None,
            factionData = factionData,
            race = npc.Race,
        };
    }
}

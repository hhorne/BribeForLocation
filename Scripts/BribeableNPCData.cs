using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Guilds;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DaggerfallWorkshop.Game.TalkManager;

public class BribeableNPCData
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

    public FactionFile.SocialGroups SocialGroup { get; set; }
    public FactionFile.GuildGroups GuildGroup { get; set; }
    public FactionFile.FactionData FactionData { get; set; }
    public Races Race { get; set; }
    public bool IsSpyMaster { get; set; }
    public bool IsGuard { get; set; }
    public NPCType NPCType { get; set; }

    public bool IsNoble => SocialGroup == FactionFile.SocialGroups.Nobility;

    public bool IsSwornToSacredorder => SacredOrders.Contains(GuildGroup);

    public static BribeableNPCData From(StaticNPC staticNPC)
    {
        var factionId = staticNPC.Data.factionID;
        if (factionId == 0)
        {
            var buildingType = GameManager.Instance.PlayerEnterExit.BuildingType;
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

        var npcData = new BribeableNPCData
        {
            NPCType = NPCType.Static,
            SocialGroup = factionData.sgroup < 5
                ? (FactionFile.SocialGroups)factionData.sgroup
                : FactionFile.SocialGroups.Merchants,
            GuildGroup = (FactionFile.GuildGroups)factionData.ggroup,
            FactionData = factionData,
            Race = TalkManager.Instance.StaticNPC.Data.race,
            IsSpyMaster = factionData.id == (int)GuildNpcServices.TG_Spymaster,
        };

        return npcData;
    }

    public static BribeableNPCData From(MobilePersonNPC npc)
    {
        FactionFile.FactionData factionData;
        int npcFactionId = GameManager.Instance.PlayerGPS.GetPeopleOfCurrentRegion();
        GameManager.Instance.PlayerEntity.FactionData.GetFactionData(npcFactionId, out factionData);

        return new BribeableNPCData
        {
            NPCType = NPCType.Mobile,
            IsGuard = npc.IsGuard,
            SocialGroup = FactionFile.SocialGroups.Commoners,
            GuildGroup = FactionFile.GuildGroups.None,
            FactionData = factionData,
            Race = npc.Race,
        };
    }

    public static BribeableNPCData FromCurrentNPC()
    {
        if (TalkManager.Instance.StaticNPC != null)
            return From(TalkManager.Instance.StaticNPC);
        else if (TalkManager.Instance.MobileNPC != null)
            return From(TalkManager.Instance.MobileNPC);

        return new BribeableNPCData();
    }
}

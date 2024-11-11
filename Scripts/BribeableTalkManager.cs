using System;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Utility;
using UnityEngine;
using Wenzil.Console;

public class BribeableTalkManager : MonoBehaviour
{
    readonly short[] knowledgeModifiers = {
        5,  7,  0,  0,  4,  1,  2, -2,  3,  7, -3,  0,  7,  2,  4,  4,  3, -2, -4, -3,
        0,  3,  5,  2,  0, -4, -3, -6, -3,  4, -7, -5,  7,  0,  1, -1,  1,  6,  4,  2
    };

    static BribeableTalkManager instance = null;
    public static BribeableTalkManager Instance
    {
        get
        {
            if (instance == null && !FindTalkManager(out instance))
            {
                GameObject go = new GameObject();
                go.name = "BribeTalkManager";
                instance = go.AddComponent<BribeableTalkManager>();
            }

            return instance;
        }
    }

    public static bool FindTalkManager(out BribeableTalkManager talkManagerOut)
    {
        talkManagerOut = FindObjectOfType<BribeableTalkManager>();
        if (talkManagerOut == null)
        {
            DaggerfallUnity.LogMessage("Could not locate BribeTalkManager GameObject instance in scene!", true);
            return false;
        }

        return true;
    }

    private void Start()
    {
        try
        {
            BribeTalkConsoleCommands.RegisterCommands();
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Error Registering Talk Console commands: {0}", ex.Message));
        }
    }

    public static class BribeTalkConsoleCommands
    {
        public static void RegisterCommands()
        {
            try
            {
                ConsoleCommandsDatabase.RegisterCommand
                (
                    TalkNpcsKnowEverything.name,
                    TalkNpcsKnowEverything.description,
                    TalkNpcsKnowEverything.usage,
                    TalkNpcsKnowEverything.Execute
                );
            }
            catch (Exception ex)
            {
                DaggerfallUnity.LogMessage(ex.Message, true);
            }
        }
    }

    public TalkManager.NPCKnowledgeAboutItem GetNPCKnowledgeAboutItem(TalkManager.ListItem topic, BribeableNPCData npcData)
    {
        // This check prevents NPCs from answering for quest resources outside the current region
        if (!CheckNPCcanKnowAboutTellMeAboutTopic(topic))
            return TalkManager.NPCKnowledgeAboutItem.DoesNotKnowAboutItem;

        if (CheckNPCisInSameBuildingAsTopic(topic) || npcData.IsSpyMaster || NPCsKnowEverything())
            return TalkManager.NPCKnowledgeAboutItem.KnowsAboutItem;

        // Fixed from classic: an NPC belonging to an organization obviously knows about it
        if (topic.questionType == TalkManager.QuestionType.OrganizationInfo &&
            GameManager.Instance.PlayerEntity.FactionData.IsFaction2RelatedToFaction1(npcData.FactionData.id, topic.factionID))
        {
            return TalkManager.NPCKnowledgeAboutItem.KnowsAboutItem;
        }

        // Make roll result be the same every time for a given NPC
        if (npcData.NPCType == TalkManager.NPCType.Mobile)
            DFRandom.Seed = (uint)TalkManager.Instance.MobileNPC.GetHashCode();
        else if (npcData.NPCType == TalkManager.NPCType.Static)
            DFRandom.Seed = (uint)TalkManager.Instance.StaticNPC.GetHashCode();

        if (topic.buildingKey != -1)
            DFRandom.Seed += (uint)topic.buildingKey;
        else if (topic.key != String.Empty)
            DFRandom.Seed += (uint)topic.key.GetHashCode();
        else
            DFRandom.Seed += (uint)topic.caption.GetHashCode();

        // Convert question type to classic index to use the knowledge modifiers array
        int classicQuestionIndex = GetClassicQuestionIndex(topic.questionType);
        int rollToBeat = knowledgeModifiers[classicQuestionIndex * 5 + (int)npcData.SocialGroup] + 10;

        int rand = DFRandom.random_range_inclusive(1, 20);
        if (rand <= rollToBeat)
            return TalkManager.NPCKnowledgeAboutItem.KnowsAboutItem;

        return TalkManager.NPCKnowledgeAboutItem.DoesNotKnowAboutItem;
    }

    bool CheckNPCcanKnowAboutTellMeAboutTopic(TalkManager.ListItem item)
    {
        Quest quest = GameManager.Instance.QuestMachine.GetQuest(item.questID);

        if (item.questionType == TalkManager.QuestionType.QuestLocation)
        {
            QuestResource questResource = quest.GetResource(item.key);
            Place place = (Place)questResource;
            if (place.SiteDetails.regionName != GameManager.Instance.PlayerGPS.CurrentRegionName)
                return false;
        }
        else if (item.questionType == TalkManager.QuestionType.QuestPerson)
        {
            QuestResource questResource = quest.GetResource(item.key);
            Person person = (Person)questResource;
            if (person.HomeRegionIndex != -1 &&
                person.HomeRegionName != GameManager.Instance.PlayerGPS.CurrentRegionName)
                return false;
        }

        return true;
    }

    bool CheckNPCisInSameBuildingAsTopic(TalkManager.ListItem item)
    {
        if (!GameManager.Instance.IsPlayerInside ||
            item.questionType != TalkManager.QuestionType.LocalBuilding && item.questionType != TalkManager.QuestionType.Person &&
            item.questionType != TalkManager.QuestionType.QuestPerson && item.questionType != TalkManager.QuestionType.QuestLocation)
            return false;

        BuildingInfo buildingInfoCurrentBuilding = GetBuildingInfoCurrentBuildingOrPalace();

        // First check if player is looking for a local building
        if (item.questionType == TalkManager.QuestionType.LocalBuilding)
        {
            if (item.buildingKey == buildingInfoCurrentBuilding.buildingKey)
            {
                item.npcInSameBuildingAsTopic = true;
                return true;
            }

            return false;
        }

        // If not looking for a local building, then player must be looking for a quest person or a quest location
        Quest quest = GameManager.Instance.QuestMachine.GetQuest(item.questID);
        QuestResource questResource = quest.GetResource(item.key);
        Person person = null;
        Symbol assignedPlaceSymbol = null;
        Place place = null;

        if (questResource is Person)
        {
            person = (Person)questResource;
            assignedPlaceSymbol = person.GetAssignedPlaceSymbol();
        }
        if (assignedPlaceSymbol != null)
        {
            place = quest.GetPlace(assignedPlaceSymbol);  // Gets actual place resource
        }
        else if (person != null)
        {
            place = person.GetHomePlace(); // get home place if no assigned place was found
        }

        // Some individuals such as The Underking have no assigned place
        if (place == null)
            return false;

        if (place.SiteDetails.regionName != GameManager.Instance.PlayerGPS.CurrentRegionName)
            return false;

        if (place.SiteDetails.locationName != GameManager.Instance.PlayerGPS.CurrentLocation.Name)
            return false;

        if (place.SiteDetails.buildingKey != 0) // building key can be 0 for palaces (so only use building key if != 0)
        {
            if (place.SiteDetails.buildingKey != buildingInfoCurrentBuilding.buildingKey)
                return false;
        }
        else // otherwise use building name
        {
            if (place.SiteDetails.buildingName != buildingInfoCurrentBuilding.name)
                return false;
        }

        item.npcInSameBuildingAsTopic = true;
        return true;
    }

    struct BuildingInfo
    {
        public string name;
        public DFLocation.BuildingTypes buildingType;
        public int buildingKey;
        public Vector2 position;
    }

    BuildingInfo GetBuildingInfoCurrentBuildingOrPalace()
    {
        BuildingInfo buildingInfoCurrentBuilding;
        var listBuildings = GetBuildingInfo();
        if (GameManager.Instance.IsPlayerInsideBuilding)
        {
            buildingInfoCurrentBuilding = listBuildings.Find(x => x.buildingKey == GameManager.Instance.PlayerEnterExit.Interior.EntryDoor.buildingKey);
        }
        else
        {
            // note Nystul :
            // resolving is not optimal here but it works - when not inside building but instead castle it will resolve via building type
            // since there is only one castle per location this finds the castle (a better way would be to have the building key of the palace entered,
            // but I could not find an easy way to determine building key of castle (PlayerGPS and PlayerEnterExit do not provide this, nor do other classes))                    
            buildingInfoCurrentBuilding = listBuildings.Find(x => x.buildingType == DFLocation.BuildingTypes.Palace);
        }
        return buildingInfoCurrentBuilding;
    }

    List<BuildingInfo> GetBuildingInfo()
    {
        DFLocation location = GameManager.Instance.PlayerGPS.CurrentLocation;
        ExteriorAutomap.BlockLayout[] blockLayout = GameManager.Instance.ExteriorAutomap.ExteriorLayout;
        DFBlock[] blocks = RMBLayout.GetLocationBuildingData(location);
        int width = location.Exterior.ExteriorData.Width;
        int height = location.Exterior.ExteriorData.Height;
        int index = 0;
        List<BuildingInfo> listBuildings = new List<BuildingInfo>();
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x, ++index)
            {
                ref readonly DFBlock block = ref blocks[index];
                BuildingSummary[] buildingsInBlock = RMBLayout.GetBuildingData(block, x, y);
                for (int i = 0; i < buildingsInBlock.Length; ++i)
                {
                    ref readonly BuildingSummary buildingSummary = ref buildingsInBlock[i];
                    try
                    {
                        BuildingInfo item;
                        item.buildingType = buildingSummary.BuildingType;
                        item.name = BuildingNames.GetName(
                            buildingSummary.NameSeed,
                            buildingSummary.BuildingType,
                            buildingSummary.FactionId,
                            TextManager.Instance.GetLocalizedLocationName(location.MapTableData.MapId, location.Name),
                            TextManager.Instance.GetLocalizedRegionName(location.RegionIndex));
                        item.buildingKey = buildingSummary.buildingKey;
                        // Compute building position in map coordinate system
                        float xPosBuilding = blockLayout[index].rect.xpos + (int)(buildingSummary.Position.x / (BlocksFile.RMBDimension * MeshReader.GlobalScale) * ExteriorAutomap.blockSizeWidth) - GameManager.Instance.ExteriorAutomap.LocationWidth * ExteriorAutomap.blockSizeWidth * 0.5f;
                        float yPosBuilding = blockLayout[index].rect.ypos + (int)(buildingSummary.Position.z / (BlocksFile.RMBDimension * MeshReader.GlobalScale) * ExteriorAutomap.blockSizeHeight) - GameManager.Instance.ExteriorAutomap.LocationHeight * ExteriorAutomap.blockSizeHeight * 0.5f;
                        item.position = new Vector2(xPosBuilding, yPosBuilding);
                        if (item.buildingKey != 0)
                            listBuildings.Add(item);
                    }
                    catch (Exception e)
                    {
                        string exceptionMessage = string.Format("exception occured in function BuildingNames.GetName (exception message: " + e.Message + @") with params: 
                                                                        seed: {0}, type: {1}, factionID: {2}, locationName: {3}, regionName: {4}",
                                                                    buildingSummary.NameSeed, buildingSummary.BuildingType, buildingSummary.FactionId, location.Name, location.RegionName);
                        DaggerfallUnity.LogMessage(exceptionMessage, true);
                    }
                }
            }
        }
        return listBuildings;
    }

    int GetClassicQuestionIndex(TalkManager.QuestionType qt)
    {
        int index = 2; // Using as default, as this gives no bonus or penalty.
        switch (qt)
        {
            case TalkManager.QuestionType.LocalBuilding:
            case TalkManager.QuestionType.Regional:
                index = 0; // == Where is Location
                break;
            case TalkManager.QuestionType.Person:
                index = 1; // == Where is Person
                break;
            case TalkManager.QuestionType.Thing: // Not used
                index = 2; // == Where is Thing
                break;
            case TalkManager.QuestionType.Work:
                index = 3; // == Where is Work
                break;
            case TalkManager.QuestionType.QuestLocation:
            case TalkManager.QuestionType.OrganizationInfo:
                index = 4; // == Tell me about Location (Also sticking OrganizationInfo here. In classic I think "OrganizationInfo" might just
                           // take whichever of location, person, item or work buttons you've last clicked on for the reaction roll.)
                break;
            case TalkManager.QuestionType.QuestPerson:
                index = 5; // == Tell me about Person
                break;
            case TalkManager.QuestionType.QuestItem:
                index = 6; // == Tell me about Thing
                break;
                // 7 == Tell me about Work (not used)
        }

        return index;
    }

    [Flags]
    private enum NPCBehaviors : byte
    {
        Default = 0,
        KnowEverything = 1,
        AlwaysFriendly = 2
    }

    NPCBehaviors consoleCommand_NPCBehaviorOverride = NPCBehaviors.Default; // used for console commands "npc_knowsEverything" and "npc_knowsUsual"

    private static class TalkNpcsKnowEverything
    {
        public static readonly string name = "talk_npcsKnowEverything";
        public static readonly string description = "NPCs know everything and do not run out of answers";
        public static readonly string usage = "talk_npcsKnowEverything";

        public static string Execute(params string[] args)
        {
            BribeableTalkManager.Instance.consoleCommand_NPCBehaviorOverride |= NPCBehaviors.KnowEverything;
            return "NPCS know everything now";
        }
    }

    bool NPCsKnowEverything()
    {
        return (consoleCommand_NPCBehaviorOverride & NPCBehaviors.KnowEverything) != 0;
    }
}

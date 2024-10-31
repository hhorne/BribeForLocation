using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System;
using System.Linq;
using DaggerfallConnect.Arena2;
using System.Reflection;
using DaggerfallConnect;
using static DaggerfallWorkshop.Game.TalkManager;

namespace BribeForLocation
{
    public class BribeableTalkWindow : DaggerfallTalkWindow
    {
        protected Button buttonBribe;

        bool IsTalkingToStaticNPC => TalkManager.Instance.StaticNPC != null;
        bool IsLocationSelected => selectedTalkCategory == TalkCategory.Location;
        bool IsWhereIsSelected => selectedTalkOption == TalkOption.WhereIs;
        TalkManager.ListItem CurrentTopic => listCurrentTopics[listboxTopic.SelectedIndex];
        BribeSettings Settings => Main.Settings;

        readonly FactionFile.GuildGroups[] respectableOrders = new[]
        {
            FactionFile.GuildGroups.KnightlyOrder,
            FactionFile.GuildGroups.HolyOrder,
        };

        public BribeableTalkWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow window)
            : base(uiManager, window)
        {
        }

        protected override void SetupButtons()
        {
            base.SetupButtons();

            buttonBribe = new Button
            {
                Name = "button_bribeForLocation",
                Position = new Vector2(118, 136),
                Size = new Vector2(67, 18),
                ToolTip = defaultToolTip,
                // needs localization
                ToolTipText = "Offer a bribe to mark a location on your map"
            };

            var talkManager = TalkManager.Instance;
            var gameManager = GameManager.Instance;
            buttonBribe.OnMouseClick += (BaseScreenComponent sender, Vector2 position) =>
            {
                if (!IsLocationTopic(CurrentTopic))
                    return;

                string answer = string.Empty;
                currentQuestion = talkManager.GetQuestionText(CurrentTopic, selectedTalkTone);

                if (IsTalkingToStaticNPC)
                {
                    var npc = GetStaticNPCData();
                    if (IsNoble(npc))
                    {
                        answer = NobleResponses.GetRandomRejection();
                    }
                    else if (IsSwornToAnOrder(npc))
                    {
                        answer = "Are you trying to insult me?";
                    }
                }
                else if(Settings.EnableNPCKnowledge)
                {
                    if (DoesNPCKnowAboutItem(CurrentTopic))
                    {
                        // add entries into the tokens(??) into the subrecords(??)
                        // so that i can expand custom macros when bribes are rejected.
                        answer = TakeBribe()
                            ? talkManager.GetKeySubjectBuildingOnMap()
                            : "You, ah...seem to be a few Septims short...";
                    }
                    else // they don't know
                    {
                        answer = talkManager.GetAnswerWhereIs(CurrentTopic);
                    }
                }
                else
                {
                    TakeBribe();
                    answer = talkManager.GetKeySubjectBuildingOnMap();
                }

                SetQuestionAnswerPairInConversationListbox(currentQuestion, answer);
            };

            mainPanel.Components.Add(buttonBribe);
        }

        private bool IsLocationTopic(TalkManager.ListItem topic)
        {
            return IsWhereIsSelected &&
                IsLocationSelected &&
                IsTopicAnItem(topic);
        }

        private bool IsTopicAnItem(TalkManager.ListItem topic)
        {
            return topic.type == TalkManager.ListItemType.Item;
        }

        private bool TakeBribe()
        {
            var playerSeptims = GameManager.Instance.PlayerEntity.GoldPieces;
            int amountRequired = GetBribeAmount();

            if (amountRequired > playerSeptims)
                return false;

            GameManager.Instance.PlayerEntity.GoldPieces -= amountRequired;
            return true;
        }

        private int GetBribeAmount()
        {
            int bribeAmount = Settings.StartingBribeAmount;

            if (Settings.ScaleByLevel)
            {
                var playerLevel = GameManager.Instance.PlayerEntity.Level;
                bribeAmount = Mathf.RoundToInt(bribeAmount * (1 + (playerLevel * 0.5f)));
            }

            return bribeAmount;
        }

        private bool IsNoble(NPCData npc)
        {
            return npc.socialGroup == FactionFile.SocialGroups.Nobility;
        }

        private bool IsSwornToAnOrder(NPCData npc)
        {
            return respectableOrders.Contains(npc.guildGroup);
        }

        static BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        private bool DoesNPCKnowAboutItem(TalkManager.ListItem listItem)
        {
            if (!Settings.EnableNPCKnowledge)
            {
                throw new Exception("[Dennis Nedry voice: Ah, ah, aaah!");
            }

            string invokedResult = string.Empty;
            TalkManager.NPCKnowledgeAboutItem knowledge;
            try
            {
                invokedResult = typeof(TalkManager)
                    .GetMethod("GetNPCKnowledgeAboutItem", nonPublicInstance)
                    .Invoke(TalkManager.Instance, new[] { listItem })
                    .ToString();

                Enum.TryParse(invokedResult, out knowledge);
            }
            catch (Exception e)
            {
                knowledge = TalkManager.NPCKnowledgeAboutItem.NotSet;
                Debug.Log(e.Message);
                Debug.Log(e.InnerException);
            }

            return knowledge == TalkManager.NPCKnowledgeAboutItem.KnowsAboutItem;
        }

        /// <summary>
        /// Combination of code from:
        ///     - TalkManager.GetStaticNPCFactionData
        ///     - TalkManager.SetTargetNPC
        /// Get NPCData for the currently selected StaticNPC.
        /// </summary>
        /// <param name="factionId">The NPC faction ID.</param>
        /// <param name="buildingType">The NPC location building type.</param>
        private NPCData GetStaticNPCData()
        {
            var talkManager = TalkManager.Instance;
            var gameManager = GameManager.Instance;
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

            var npcData = new NPCData
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
    }
}

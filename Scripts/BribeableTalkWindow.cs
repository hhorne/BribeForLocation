using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System;
using System.Linq;
using DaggerfallConnect.Arena2;
using System.Reflection;

namespace BribeForLocation
{
    public class BribeableTalkWindow : DaggerfallTalkWindow
    {
        protected Button buttonBribe;

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
            buttonBribe.OnMouseClick += (BaseScreenComponent sender, Vector2 position) =>
            {
                if (!IsLocationTopic(CurrentTopic))
                    return;

                currentQuestion = talkManager.GetQuestionText(CurrentTopic, selectedTalkTone);
                string answer = string.Empty;

                if (Settings.EnableGetNPCData)
                {
                    var npc = GetNPCData();
                    if (npc.socialGroup == FactionFile.SocialGroups.Nobility)
                    {
                        answer = NobleResponses.GetRandomRejection();
                    }
                    else if (respectableOrders.Contains(npc.guildGroup))
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

        private TalkManager.NPCData GetNPCData()
        {
            if (!Settings.EnableGetNPCData)
            {
                throw new Exception("[Dennis Nedry voice: Ah, ah, aaah!");
            }

            var talkManager = TalkManager.Instance;
            TalkManager.NPCData npcData;
            try
            {
                npcData = (TalkManager.NPCData)talkManager
                    .GetType()
                    .GetField("npcData", nonPublicInstance)
                    .GetValue(talkManager);
            }
            catch (Exception e)
            {
                npcData = new TalkManager.NPCData();
                Debug.Log(e.Message);
                Debug.Log(e.InnerException);
            }

            return npcData;
        }
    }
}

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
        bool scaleByLevel => BribeForLocation.Main.ScaleByLevel;
        int bribeAmount => BribeForLocation.Main.BribeAmount;

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
                var npc = GetNPCData();
                bool inSameGuild = GameManager
                    .Instance
                    .GuildManager
                    .GetGuild(npc.guildGroup)
                    .IsMember();

                if (npc.socialGroup == FactionFile.SocialGroups.Nobility)
                {
                    answer = NobleResponses.GetRandomRejection();
                }
                else if (respectableOrders.Contains(npc.guildGroup))
                {
                    answer = "Are you trying to insult me?";
                }
                else if(DoesNPCKnowAboutItem(CurrentTopic))
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
            var playerLevel = GameManager.Instance.PlayerEntity.Level;
            var playerSeptims = GameManager.Instance.PlayerEntity.GoldPieces;
            int amountRequired = scaleByLevel
                ? Mathf.RoundToInt(bribeAmount * (1 + (playerLevel * 0.5f)))
                : bribeAmount;

            Debug.Log("Bribe amount: " + amountRequired);
            Debug.Log("Scaled?: " + scaleByLevel);
            if (amountRequired > playerSeptims)
                return false;

            GameManager.Instance.PlayerEntity.GoldPieces -= amountRequired;
            return true;
        }

        static BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        private bool DoesNPCKnowAboutItem(TalkManager.ListItem listItem)
        {
            var invokedResult = typeof(TalkManager)
                .GetMethod("GetNPCKnowledgeAboutItem", nonPublicInstance)
                .Invoke(TalkManager.Instance, new[] { listItem })
                .ToString();

            Enum.TryParse(invokedResult, out TalkManager.NPCKnowledgeAboutItem knowledge);

            return knowledge == TalkManager.NPCKnowledgeAboutItem.KnowsAboutItem;
        }

        private TalkManager.NPCData GetNPCData()
        {
            var talkManager = TalkManager.Instance;
            return (TalkManager.NPCData)talkManager
                .GetType()
                .GetField("npcData", nonPublicInstance)
                .GetValue(talkManager);
        }
    }
}

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility.AssetInjection;

namespace BribeForLocation
{
    public class BribeableTalkWindow : DaggerfallTalkWindow
    {
        readonly string defaultToolTipText = "Offer a bribe to mark a location on your map";
        protected Button buttonBribe;
        protected string bribeButtonEnabledImgName = "bribe-button-enabled.bmp";
        protected string bribeButtonDisabledImgName = "bribe-button-disabled.bmp";
        protected Texture2D bribeButtonHighlightedTexture;
        protected Texture2D bribeButtonGrayedOutTexture;

        BribeSystem bribeSystem;

        public BribeableTalkWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow window)
            : base(uiManager, window)
        {
            bribeSystem = new BribeSystem();
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
                ToolTipText = defaultToolTipText
            };

            TextureReplacement.TryImportImage(bribeButtonEnabledImgName, true, out bribeButtonHighlightedTexture);
            TextureReplacement.TryImportImage(bribeButtonDisabledImgName, true, out bribeButtonGrayedOutTexture);

            buttonBribe.BackgroundTexture = bribeButtonGrayedOutTexture;
            buttonBribe.OnMouseClick += OnBribeClickHandler;

            mainPanel.Components.Add(buttonBribe);
        }

        protected override void ListboxTopic_OnSelectItem()
        {
            base.ListboxTopic_OnSelectItem();
            UpdateBribeButtonTexture();
        }

        private bool CanBribe() => IsBribeableTopic() && CanAffordBribe();

        private bool CanAffordBribe() => bribeSystem.CanBribe(listCurrentTopics[listboxTopic.SelectedIndex]);

        private void UpdateBribeButtonTexture()
        {
            if (CanBribe())
            {
                buttonBribe.BackgroundTexture = bribeButtonHighlightedTexture;
                buttonBribe.ToolTipText = defaultToolTipText;
            }
            else
            {
                if (!IsBribeableTopic())
                {
                    buttonBribe.ToolTipText = "You don't think coin will get you anything on this topic.";
                }
                else if (!CanAffordBribe())
                {
                    buttonBribe.ToolTipText = "You don't have enough coin.";
                }

                buttonBribe.BackgroundTexture = bribeButtonGrayedOutTexture;
            }
        }

        bool IsBribeableTopic()
        {
            return selectedTalkOption == TalkOption.WhereIs &&
                listCurrentTopics[listboxTopic.SelectedIndex].type == TalkManager.ListItemType.Item;
        }

        void OnBribeClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            if (CanBribe())
            {
                var topic = listCurrentTopics[listboxTopic.SelectedIndex];
                var currentNpc = BribeableNPCData.FromCurrentNPC();
                string answer = bribeSystem.GetBribeResponse(currentNpc, topic);
                currentQuestion = TalkManager.Instance.GetQuestionText(topic, selectedTalkTone);

                SetQuestionAnswerPairInConversationListbox(currentQuestion, answer);
                UpdateBribeButtonTexture();
            }
        }
    }
}

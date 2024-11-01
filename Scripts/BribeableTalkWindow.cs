using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility.AssetInjection;
using System.IO;

namespace BribeForLocation
{
    public class BribeableTalkWindow : DaggerfallTalkWindow
    {
        protected Button buttonBribe;
        protected string bribeButtonEnabledImgName = "bribe-button-enabled";
        protected string bribeButtonDisabledImgName = "bribe-button-disabled.bmp";
        protected Texture2D bribeButtonHighlightedTexture;
        protected Texture2D bribeButtonGrayedOutTexture;

        TalkManager.ListItem CurrentTopic => listCurrentTopics[listboxTopic.SelectedIndex];
        BribeSystem bribeSystem;

        public BribeableTalkWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow window)
            : base(uiManager, window)
        {
            bribeSystem = new BribeSystem(GameManager.Instance.PlayerEntity);
        }

        protected override void SetupButtons()
        {
            base.SetupButtons();

            TextureReplacement.TryImportImage(bribeButtonEnabledImgName, true, out bribeButtonHighlightedTexture);
            TextureReplacement.TryImportImage(bribeButtonDisabledImgName, true, out bribeButtonGrayedOutTexture);

            buttonBribe = new Button
            {
                Name = "button_bribeForLocation",
                Position = new Vector2(118, 136),
                Size = new Vector2(67, 18),
                ToolTip = defaultToolTip,
                ToolTipText = "Offer a bribe to mark a location on your map"
            };

            buttonBribe.BackgroundTexture = bribeButtonGrayedOutTexture;
            buttonBribe.OnMouseClick += OnBribeClickHandler;

            mainPanel.Components.Add(buttonBribe);
        }

        void OnBribeClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            var talkManager = TalkManager.Instance;
            if (!IsLocationTopic(CurrentTopic))
                return;

            currentQuestion = talkManager.GetQuestionText(CurrentTopic, selectedTalkTone);

            string answer = talkManager.BribeNPC(CurrentTopic, bribeSystem);

            SetQuestionAnswerPairInConversationListbox(currentQuestion, answer);
        }

        bool IsLocationTopic(TalkManager.ListItem topic)
        {
            return selectedTalkOption == TalkOption.WhereIs &&
                selectedTalkCategory == TalkCategory.Location &&
                topic.IsItem();
        }

        protected override void ListboxTopic_OnSelectItem()
        {
            base.ListboxTopic_OnSelectItem();
            if (IsLocationTopic(CurrentTopic) && bribeSystem.CanBribe())
            {
                buttonBribe.BackgroundTexture = bribeButtonHighlightedTexture;
            }
            else
            {
                buttonBribe.BackgroundTexture = bribeButtonGrayedOutTexture;
            }
        }
    }
}

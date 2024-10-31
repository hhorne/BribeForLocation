using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace BribeForLocation
{
    public class BribeableTalkWindow : DaggerfallTalkWindow
    {
        protected Button buttonBribe;

        TalkManager.ListItem CurrentTopic => listCurrentTopics[listboxTopic.SelectedIndex];

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
                ToolTipText = "Offer a bribe to mark a location on your map"
            };

            buttonBribe.OnMouseClick += OnBribeClickHandler;

            mainPanel.Components.Add(buttonBribe);
        }

        void OnBribeClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            var talkManager = TalkManager.Instance;
            if (!IsLocationTopic(CurrentTopic))
                return;

            currentQuestion = talkManager.GetQuestionText(CurrentTopic, selectedTalkTone);

            string answer = talkManager.BribeNPC(CurrentTopic);

            SetQuestionAnswerPairInConversationListbox(currentQuestion, answer);
        }

        bool IsLocationTopic(TalkManager.ListItem topic)
        {
            return selectedTalkOption == TalkOption.WhereIs &&
                selectedTalkCategory == TalkCategory.Location &&
                topic.IsItem();
        }
    }
}

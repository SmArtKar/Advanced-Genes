using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using static Mono.Security.X509.X520;

namespace Advanced_Genes
{
    [StaticConstructorOnStartup]
    internal class Dialog_CreateHivemind : Window
    {
        private static Texture2D backgroundIcon = ContentFinder<Texture2D>.Get("UI/Icons/UI_Background");
        public static Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-.]*$");
        private Pawn pawn;
        private Hediff_Hivemind hediff;
        private Texture2D hiveIcon;

        private string hiveName;

        public const int maxNameLength = 30;

        public Dialog_CreateHivemind(Pawn pawn, Hediff_Hivemind hediff, Texture2D hiveIcon)
        {
            this.pawn = pawn;
            this.hediff = hediff;
            this.hiveIcon = hiveIcon;
            this.forcePause = true;
            hiveName = hediff.getHivemindName;
        }

        public override Vector2 InitialSize => new(500f, 157f);

        public override void DoWindowContents(Rect inRect)
        {
            inRect = inRect.ContractedBy(10f, 10f);

            Text.Font = GameFont.Medium;
            Widgets.Label(new(0f, 0f, 245f, 30f), "Create a new hivemind");
            Text.Font = GameFont.Small;
            Widgets.Label(new(0f, 45f, 245f, 20f), "Name:");

            Rect backgroundRect = new(inRect.width - 59f, 0f, 74f, 74f);
            Rect imageRect = new(inRect.width - 54f, 5f, 64f, 64f);
            GUI.DrawTexture(backgroundRect, backgroundIcon);
            GUI.DrawTexture(imageRect, hiveIcon);

            Rect nameInputRect = new(60f, 40f, 300f, 30f);

            string text = Widgets.TextField(nameInputRect, hiveName);
            if (text.Length <= maxNameLength && ValidNameRegex.IsMatch(text))
            {
                hiveName = text;
            }

            Rect createButton = new(0f, 85f, 175f, 35f);
            if (Widgets.ButtonText(createButton, "Create hivemind"))
            {
                hediff.createNewHivemind(hiveName);
                InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Hivemind));
                Close();
            }

            Rect cancelButton = new(185f, 85f, 175f, 35f);

            if (Widgets.ButtonText(cancelButton, "Cancel"))
            {
                Close();
            }
        }
    }
}

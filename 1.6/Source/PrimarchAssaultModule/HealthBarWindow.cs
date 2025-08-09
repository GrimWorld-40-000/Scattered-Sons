using PrimarchAssault.External;
using PrimarchAssault.Settings;
using RimWorld;
using UnityEngine;
using Verse;

namespace PrimarchAssault
{
    public class HealthBarWindow: Window
    {
        public HealthBarWindow()
        {
            doWindowBackground = false;
            doCloseX = false;
            windowRect = new Rect(Current.Camera.scaledPixelWidth / (float)2 - 1000, 30, 2000, 150);
            draggable = SettingsTabRecord_PrimarchAssault.SettingsRecord.CanHealthbarBeMoved;
            resizeable = SettingsTabRecord_PrimarchAssault.SettingsRecord.CanHealthbarBeMoved;
            absorbInputAroundWindow = false;
            closeOnAccept = false;
            closeOnCancel = false;
            closeOnClickedOutside = false;
            drawShadow = false;
            focusWhenOpened = false;
            preventCameraMotion = false;
            layer = WindowLayer.GameUI;
        }

        public override Vector2 InitialSize => new Vector2(InitialWidth, InitialHeight);

        private float _healthPercent;
        private float _shieldPercent;
        public ChallengeDef ChallengeDef;
        public int CurrentPawn;
        private Rect _healthBarRelative;
        private Rect _shieldBarRelative;

        private const int InitialWidth = 2000;
        private const int InitialHeight = 150;

        private static readonly Color ShieldColor = new Color(.65f, .78f, 1f);
        private static readonly Color HealthColor = new Color(.81f, .24f, .12f);
        private static readonly Color ShieldBGColor = new Color(.35f, .4f, .5f);
        private static readonly Color HealthBGColor = new Color(.4f, .12f, .06f);

        private float ScaleByWidth(float originalWidth)
        {
            return originalWidth * (windowRect.width / InitialWidth);
        }
        
        private float ScaleByHeight(float originalHeight)
        {
            return originalHeight * (windowRect.height / InitialHeight);
        }
        
        public override void DoWindowContents(Rect inRect)
        {

            Rect barRect = new Rect
            {
                width = ScaleByWidth(1280),
                height = ScaleByHeight(128),
                center = inRect.center
            };

            if (ChallengeDef.HealthBarIcon == null) return;
            Rect shieldBar = new Rect(barRect.xMin + ScaleByWidth(_shieldBarRelative.x), barRect.yMin + ScaleByHeight(_shieldBarRelative.y), ScaleByWidth(_shieldBarRelative.width), ScaleByHeight(_shieldBarRelative.height));
            Rect healthBar = new Rect(barRect.xMin + ScaleByWidth(_healthBarRelative.x), barRect.yMin + ScaleByHeight(_healthBarRelative.y), ScaleByWidth(_healthBarRelative.width), ScaleByHeight(_healthBarRelative.height));
            Widgets.DrawRectFast(healthBar, HealthBGColor);
            Widgets.DrawRectFast(healthBar.LeftPart(_healthPercent), HealthColor);
            Widgets.DrawRectFast(shieldBar, ShieldBGColor);
            Widgets.DrawRectFast(shieldBar.LeftPart(_shieldPercent), ShieldColor);
            GUI.DrawTexture(barRect, ChallengeDef.HealthBarIcon);
                
        }

        public void UpdateIfWilling(int championId, float healthPercent, float shieldPercent, Rect healthBarRelative, Rect shieldBarRelative)
        {
            if (championId != CurrentPawn) return; 
            _healthPercent = healthPercent;
            _shieldPercent = shieldPercent;
            _healthBarRelative = healthBarRelative;
            _shieldBarRelative = shieldBarRelative;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Randomizer.Util
{
    public static class Ref
    {
        private static HeroController _hc;
        private static GameManager _gm;
        private static UIManager _ui;
        private static InputHandler _input;

        public static PlayerData PD => PlayerData.instance;
        public static HeroController HC => _hc ??= HeroController.instance;
        public static GameManager GM => _gm ??= GameManager.instance;
        public static UIManager UI => _ui ??= UIManager.instance;
        public static InputHandler Input => _input ??= GM.inputHandler;
    }
}

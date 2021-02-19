using Randomizer.Serialized;

namespace Randomizer
{
    public static class ItemCallbacks
    {
        public static void UpdateGeoCounter(ItemStage stage)
            => HeroController.instance.geoCounter.AddGeo(stage.IntActions[0].Value);

        public static void CheckMaskShards(ItemStage stage)
        {
            if (PlayerData.instance.heartPieces < 4 || PlayerData.instance.maxHealthBase >= PlayerData.instance.maxHealthCap)
            {
                return;
            }

            HeroController.instance.AddToMaxHealth(1);
            PlayMakerFSM.BroadcastEvent("MAX HP UP");
            PlayMakerFSM.BroadcastEvent("HERO HEALED FULL");
            if (PlayerData.instance.maxHealthBase < PlayerData.instance.maxHealthCap)
            {
                PlayerData.instance.heartPieces -= 4;
                CheckMaskShards(stage);
            }
        }

        public static void CheckVesselFragments(ItemStage stage)
        {
            if (PlayerData.instance.vesselFragments < 3 || PlayerData.instance.MPReserveMax >= PlayerData.instance.MPReserveCap)
            {
                return;
            }

            HeroController.instance.AddToMaxMPReserve(33);
            PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
            if (PlayerData.instance.MPReserveMax < PlayerData.instance.MPReserveCap)
            {
                PlayerData.instance.vesselFragments -= 3;
                CheckVesselFragments(stage);
            }
        }

        public static void MakeGrubNoise(ItemStage stage)
        {

        }

        public static void CheckNailArts(ItemStage stage)
            => PlayerData.instance.hasAllNailArts = PlayerData.instance.hasCyclone
            && PlayerData.instance.hasUpwardSlash && PlayerData.instance.hasDashSlash;

        public static void CheckNotches(ItemStage stage)
        {
            PlayerData pd = PlayerData.instance;
            pd.CountCharms();
            int charms = pd.charmsOwned;
            int notches = pd.charmSlots;

            if (!pd.salubraNotch1 && charms >= 5)
            {
                pd.salubraNotch1 = true;
                notches++;
            }

            if (!pd.salubraNotch2 && charms >= 10)
            {
                pd.salubraNotch2 = true;
                notches++;
            }

            if (!pd.salubraNotch3 && charms >= 18)
            {
                pd.salubraNotch3 = true;
                notches++;
            }

            if (!pd.salubraNotch4 && charms >= 25)
            {
                pd.salubraNotch4 = true;
                notches++;
            }

            pd.SetInt(nameof(PlayerData.charmSlots), notches);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using HKLang = Randomizer.Patches.Language;

namespace Randomizer.Util
{
    public static class Lang
    {
        private static readonly Dictionary<KeyValuePair<string, string>, string> Overrides
            = new Dictionary<KeyValuePair<string, string>, string>();

        public static void Add(string sheet, string key, string text)
        {
            if (string.IsNullOrEmpty(sheet)) throw new ArgumentNullException(nameof(sheet));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (text is null) throw new ArgumentNullException(nameof(text));

            Overrides[new KeyValuePair<string, string>(sheet, key)] = text;
        }

        public static string Get(string sheet, string key)
        {
            if (string.IsNullOrEmpty(sheet)) throw new ArgumentNullException(nameof(sheet));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            if (!Overrides.TryGetValue(new KeyValuePair<string, string>(sheet, key), out string text))
            {
                if (sheet == "Prices" && int.TryParse(key, out int cost))
                {
                    text = cost.ToString();
                }
                else if (sheet == "UI" && key.StartsWith("RANDOMIZER_NAME_ESSENCE_"))
                {
                    text = key.Replace("RANDOMIZER_NAME_ESSENCE_", "") + " " + Get(sheet, "RANDOMIZER_NAME_ESSENCE");
                }
                else
                {
                    text = HKLang.orig_Get(key, sheet);
                }
            }

            return text.Replace("<br>", "\n");
        }

        public static string Get(string sheetDotKey)
        {
            if (sheetDotKey is null) throw new ArgumentNullException(nameof(sheetDotKey));

            string[] split = sheetDotKey.Split('.');
            if (split.Length != 2) throw new InvalidOperationException($"Input {sheetDotKey} is not of the form 'sheetTitle.key'");

            return Get(split[0], split[1]);
        }

        public static bool Has(string sheet, string key)
            => Overrides.ContainsKey(new KeyValuePair<string, string>(sheet, key)) ? true : HKLang.Has(key, sheet);

        [CompilerGenerated]
        static Lang()
        {
            Add("UI", "RANDOMIZER_EMPTY", "");
            Add("Prompts", "RANDOMIZER_EMPTY", "");
            Add("UI", "RANDOMIZER_CHARM_DESC_1", "Just think of all the time you could save collecting geo with this.\n\nTime is money, right? Definitely worth it.");
            Add("UI", "RANDOMIZER_CHARM_DESC_2", "If you can't keep your bearings without this you should probably just stop playing randomizer.");
            Add("UI", "RANDOMIZER_CHARM_DESC_3", "What's better than being rewarded for getting hit? Amazing charm if you suck.");
            Add("UI", "RANDOMIZER_CHARM_DESC_4", "Now all you need is quick slash, longnail, and mark of pride. Reddit's favorite charm build!\n\nReddit is never wrong, after all.");
            Add("UI", "RANDOMIZER_CHARM_DESC_5", "It's like dreamshield, but cheaper and better at blocking damage.");
            Add("UI", "RANDOMIZER_CHARM_DESC_6", "Sure, you COULD stay at 1 health with this, but a real gamer would stay at 2.\n\nDon't forget to pogo everything, too.");
            Add("UI", "RANDOMIZER_CHARM_DESC_7", "Mickely's favorite charm.");
            Add("UI", "RANDOMIZER_CHARM_DESC_8", "With this on, you'll be able to tank a whole two more hits. Amazing.");
            Add("UI", "RANDOMIZER_CHARM_DESC_9", "Homothety's favorite charm.");
            Add("UI", "RANDOMIZER_CHARM_DESC_10", "Haha poo.");
            Add("UI", "RANDOMIZER_CHARM_DESC_11", "RIP Flukenest 2017-2018.");
            Add("UI", "RANDOMIZER_CHARM_DESC_12", "It's a good thing thorn warps got patched so fast. Wouldn't want any cheaters in my randomizer.");
            Add("UI", "RANDOMIZER_CHARM_DESC_13", "Three notches to slightly extend the range of my weakest attack? Sign me up!");
            Add("UI", "RANDOMIZER_CHARM_DESC_14", "You ever feel like you don't walk into enemies during combat enough? Well I have just the charm for you!");
            Add("UI", "RANDOMIZER_CHARM_DESC_15", "If you've got Kerr's charm modifications on, buying this could be a real blast.");
            Add("UI", "RANDOMIZER_CHARM_DESC_16", "Such an amazing charm it has its own mod dedicated to it. What fool wouldn't buy this?");
            Add("UI", "RANDOMIZER_CHARM_DESC_17", "Man it'd be a real bummer if you had to buy this garbage to leave Crossroads.");
            Add("UI", "RANDOMIZER_CHARM_DESC_18", "This is technically enough to kill baldurs, but you're not that crazy.\n\nAt least I hope you aren't.");
            Add("UI", "RANDOMIZER_CHARM_DESC_19", "You obviously want to buy this. But is it worth the cost?\n\n(Spoilers: yes.)");
            Add("UI", "RANDOMIZER_CHARM_DESC_20", "You'd have to be really hurting for damage to buy this, of all things.");
            Add("UI", "RANDOMIZER_CHARM_DESC_21", "Who would ever use this charm? The effect is amazing, but come on. Four notches?");
            Add("UI", "RANDOMIZER_CHARM_DESC_22", "Back in my day we had to jump through tunnels of spikes to get something like this.");
            Add("UI", "RANDOMIZER_CHARM_DESC_23", "Wow two hearts? That's eight mask shards you don't have to collect.");
            Add("UI", "RANDOMIZER_CHARM_DESC_24", "Gotta spend money to make money. I'm sure it'll pay for itself in no time at all.");
            Add("UI", "RANDOMIZER_CHARM_DESC_25", "Wow this is a steal! Unbreakable normally adds 15k, so really you'd be a fool to not buy at this price.");
            Add("UI", "RANDOMIZER_CHARM_DESC_26", "Who uses nail arts? Probably nobody now that the dash slash skips are gone.");
            Add("UI", "RANDOMIZER_CHARM_DESC_27", "What do you mean I can't heal with this on?");
            Add("UI", "RANDOMIZER_CHARM_DESC_28", "You want to turn into a snail? Of course not. This is a piece of garbage.\n\nSave your money for something else.");
            Add("UI", "RANDOMIZER_CHARM_DESC_29", "Basically required to get through the hive alive. Better hope nothing is in there.");
            Add("UI", "RANDOMIZER_CHARM_DESC_30", "Most people who play probably don't even know the dream nail gives you soul.\n\nMaybe that's why this is only one notch?");
            Add("UI", "RANDOMIZER_CHARM_DESC_31", "Not worth the money unless you're using glitches. You're not a cheater, are you?");
            Add("UI", "RANDOMIZER_CHARM_DESC_32", "Time to tank and spam nail. Strategy of champions.");
            Add("UI", "RANDOMIZER_CHARM_DESC_33", "You know how this is normally surrounded by mistakes? Reminds me of anime.");
            Add("UI", "RANDOMIZER_CHARM_DESC_34", "You ever wonder what it would be like if healing took so long you just get hit?");
            Add("UI", "RANDOMIZER_CHARM_DESC_35", "Play lightbringer.");
            Add("UI", "RANDOMIZER_CHARM_DESC_36_L", "That's right, I'm not even going to give you the full charm. That's how little I value your business.");
            Add("UI", "RANDOMIZER_CHARM_DESC_36_R", "You want half of a charm? Well today is your lucky day!");
            Add("UI", "RANDOMIZER_CHARM_DESC_36_Void", "A 5-notch charm which opens a hole in the ground, and would break shade skip logic everywhere if not for the efforts of a heroic modder somewhere.");
            Add("UI", "RANDOMIZER_CHARM_DESC_37", "Go faster! But only when on the ground. And also only when walking. Oh and also it's a very minor speed boost.");
            Add("UI", "RANDOMIZER_CHARM_DESC_38", "This should really be a one notch charm. Hell, even at one, I wouldn't use this.");
            Add("UI", "RANDOMIZER_CHARM_DESC_39", "This is a good charm after the buffs in Lifeblood, right?");
            Add("UI", "RANDOMIZER_CHARM_DESC_40", "Most people would pay money to get rid of this trash, yet here you are contemplating spending money on it.");
            Add("UI", "RANDOMIZER_INV_NAME_GODFINDER", "Godtuner");
            Add("UI", "RANDOMIZER_INV_DESC_GODFINDER", "Ignore the name; this is actually Mantis Claw. Or maybe Dream Nail. Oh, by the way, no refunds.");
            Add("UI", "RANDOMIZER_COLLECTOR_MAP", "Collector's Map");
            Add("UI", "RANDOMIZER_GET_COLLECTOR_MAP", "Don't think about this as a map. Think about it as a statement about who you are as a person.");
            Add("UI", "RANDOMIZER_NAME_LURIEN", "Lurien");
            Add("UI", "RANDOMIZER_DESC_LURIEN", "This should be at least 10 times more expensive than it is.");
            Add("UI", "RANDOMIZER_NAME_MONOMON", "Monomon");
            Add("UI", "RANDOMIZER_DESC_MONOMON", "This should be at least 10 times more expensive than it is.");
            Add("UI", "RANDOMIZER_NAME_HERRAH", "Herrah");
            Add("UI", "RANDOMIZER_DESC_HERRAH", "This should be at least 10 times more expensive than it is.");
            Add("UI", "RANDOMIZER_NAME_DREAMER", "Dreamer");
            Add("UI", "RANDOMIZER_DESC_DREAMER", "This should be at least 10 times more expensive than it is.");
            Add("UI", "RANDOMIZER_NAME_WORLD_SENSE", "World Sense");
            Add("UI", "RANDOMIZER_DESC_WORLD_SENSE", "Vessel. Though bound, you shall know the state of the world. Hallownest will be whole again.");
            Add("UI", "RANDOMIZER_GET_WORLD_SENSE_1", "Vessel. Though bound, you shall know the state of the world.");
            Add("UI", "RANDOMIZER_GET_WORLD_SENSE_2", "Hallownest will be whole again.");
            Add("UI", "RANDOMIZER_NAME_MASK_SHARD", "Mask Shard");
            Add("UI", "RANDOMIZER_DESC_MASK_SHARD", "It's on the cheap now. Some assembly required.");
            Add("UI", "RANDOMIZER_NAME_VESSEL_FRAGMENT", "Vessel Fragment");
            Add("UI", "RANDOMIZER_DESC_VESSEL_FRAGMENT", "It's on the cheap now. Some assembly required.");
            Add("UI", "RANDOMIZER_NAME_PALE_ORE", "Pale Ore");
            Add("UI", "RANDOMIZER_DESC_PALE_ORE", "GIVE THIS TO THE NAILSMITH");
            Add("UI", "RANDOMIZER_NAME_CHARM_NOTCH", "Charm Notch");
            Add("UI", "RANDOMIZER_DESC_CHARM_NOTCH", "DON'T DROP IT");
            Add("UI", "RANDOMIZER_NAME_ARCANE_EGG", "Arcane Egg");
            Add("UI", "RANDOMIZER_NAME_ESSENCE", "Essence");
            Add("UI", "RANDOMIZER_DESC_ESSENCE", "Fresh organic DREAM PLANT ORBS sourced from local independent DREAM PLANT farms. Please support your hardworking neighborhood DREAM MOTHS.");
            Add("UI", "RANDOMIZER_NAME_LIFEBLOOD_COCOON_SMALL", "6 lifeblood masks");
            Add("UI", "RANDOMIZER_NAME_LIFEBLOOD_COCOON_LARGE", "9 lifeblood masks");
            Add("UI", "RANDOMIZER_DESC_LIFEBLOOD_COCOON", "Limited edition Lifeblood masks - get them while they last!");
            Add("UI", "RANDOMIZER_NAME_GRIMMKIN_FLAME", "Grimmkin Flame");
            Add("UI", "RANDOMIZER_DESC_GRIMMKIN_FLAME", "Hot new addition to your randomizer!");
            Add("UI", "RANDOMIZER_NAME_1GEO", "1 geo");
            Add("UI", "RANDOMIZER_NAME_15GEO", "15 geo");
            Add("UI", "RANDOMIZER_NAME_17GEO", "17 geo");
            Add("UI", "RANDOMIZER_NAME_22GEO", "22 geo");
            Add("UI", "RANDOMIZER_NAME_24GEO", "24 geo");
            Add("UI", "RANDOMIZER_NAME_25GEO", "25 geo");
            Add("UI", "RANDOMIZER_NAME_26GEO", "26 geo");
            Add("UI", "RANDOMIZER_NAME_30GEO", "30 geo");
            Add("UI", "RANDOMIZER_NAME_35GEO", "35 geo");
            Add("UI", "RANDOMIZER_NAME_44GEO", "44 geo");
            Add("UI", "RANDOMIZER_NAME_56GEO", "56 geo");
            Add("UI", "RANDOMIZER_NAME_80GEO", "80 geo");
            Add("UI", "RANDOMIZER_NAME_85GEO", "85 geo");
            Add("UI", "RANDOMIZER_NAME_150GEO", "150 geo");
            Add("UI", "RANDOMIZER_NAME_160GEO", "160 geo");
            Add("UI", "RANDOMIZER_NAME_200GEO", "200 geo");
            Add("UI", "RANDOMIZER_NAME_380GEO", "380 geo");
            Add("UI", "RANDOMIZER_NAME_420GEO", "420 geo");
            Add("UI", "RANDOMIZER_NAME_620GEO", "620 geo");
            Add("UI", "RANDOMIZER_NAME_655GEO", "655 geo");
            Add("UI", "RANDOMIZER_DESC_GEO", "We're having a fire sale today. Go ahead and pick up your rebate already.");
            Add("UI", "RANDOMIZER_NAME_SOUL", "Soul Refill");
            Add("UI", "RANDOMIZER_DESC_SOUL", "Comes with a cup, lid, and sippy straw. No additional refills.");
            Add("UI", "RANDOMIZER_STAG_NAME_DIRTMOUTH", "Dirtmouth Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_CROSSROADS", "Crossroads Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_GREENPATH", "Greenpath Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_QUEENSSTATION", "Queen's Station Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_QUEENSGARDENS", "Queen's Gardens Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_STOREROOMS", "City Storerooms Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_KINGSSTATION", "King's Station Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_RESTINGGROUNDS", "Resting Grounds Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_DEEPNEST", "Distant Village Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_HIDDENSTATION", "Hidden Station Stag");
            Add("UI", "RANDOMIZER_STAG_NAME_STAGNEST", "Stag Nest Stag");
            Add("UI", "RANDOMIZER_STAG_DESC", "IDK, seems pretty useful if you ask me.");
            Add("UI", "RANDOMIZER_NAME_GRUB", "A grub");
            Add("UI", "RANDOMIZER_DESC_GRUB", "A bottled grub. Perfect for making juice, sauce, or any tasty meal really. We only have green ones in stock though.");
            Add("UI", "RANDOMIZER_MAP_NAME_DIRTMOUTH", "Dirtmouth Map");
            Add("UI", "RANDOMIZER_MAP_NAME_CROSSROADS", "Crossroads Map");
            Add("UI", "RANDOMIZER_MAP_NAME_GREENPATH", "Greenpath Map");
            Add("UI", "RANDOMIZER_MAP_NAME_FOG_CANYON", "Fog Canyon Map");
            Add("UI", "RANDOMIZER_MAP_NAME_FUNGAL_WASTES", "Fungal Wastes Map");
            Add("UI", "RANDOMIZER_MAP_NAME_DEEPNEST", "Deepnest Map");
            Add("UI", "RANDOMIZER_MAP_NAME_ABYSS", "Ancient Basin Map");
            Add("UI", "RANDOMIZER_MAP_NAME_OUTSKIRTS", "Kingdom's Edge Map");
            Add("UI", "RANDOMIZER_MAP_NAME_CITY", "City of Tears Map");
            Add("UI", "RANDOMIZER_MAP_NAME_WATERWAYS", "Royal Waterways Map");
            Add("UI", "RANDOMIZER_MAP_NAME_CLIFFS", "Howling Cliffs Map");
            Add("UI", "RANDOMIZER_MAP_NAME_MINES", "Crystal Peak Map");
            Add("UI", "RANDOMIZER_MAP_NAME_ROYAL_GARDENS", "Queen's Gardens Map");
            Add("UI", "RANDOMIZER_MAP_NAME_RESTING_GROUNDS", "Resting Grounds Map");
            Add("UI", "RANDOMIZER_NAME_FOCUS", "Focus");
            Add("UI", "RANDOMIZER_DESC_FOCUS", "Press and hold the slow cast button to heal.");
            Add("Prompts", "RANDOMIZER_DESC_FOCUS", "Press and hold the slow cast button to heal.");
            Add("UI", "RANDOMIZER_SHOP_DESC_FOCUS", "Hahahahahahahahahahahaha. No.");
            Add("Prompts", "RANDOMIZER_MEME", "Lol, you thought this was something good for a sec.");
            Add("Prompts", "RANDOMIZER_BUTTON_DESC", "You already know what to press");
            Add("Prompts", "RANDOMIZER_EMPTY", "");
            Add("Prompts", "NOT_ENOUGH_GEO", "Not enough.");
            Add("Elderbug", "ELDERBUG_FLOWER_REQUEST", "F-for me..? Senpai you're so kind.");
            Add("Elderbug", "ELDERBUG_FLOWER_REREQUEST", "F-for me..? Senpai you're so kind.");
            Add("Elderbug", "ELDERBUG_FLOWER_ACCEPT", "Thank you senpai!");
            Add("Elderbug", "ELDERBUG_FLOWER_DECLINE", "O-oh...");
            Add("Prompts", "ELDERBUG_FLOWER", "Give Elderbug-chan the flower?");
            Add("Hornet", "HORNET_GREENPATH", "I'm not so hard? Are you kidding me?");
            Add("MainMenu", "VIDEO_VSYNC", "Input Lag:");
            Add("MainMenu", "DESC_VSYNC", "Leave on to make the game unplayable.");
            Add("Titles", "DIRTMOUTH_MAIN", "Elderbug");
            Add("Titles", "DIRTMOUTH_SUB", "is a cool dude");
        }
    }
}

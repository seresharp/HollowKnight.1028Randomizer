using System.Runtime.CompilerServices;
using Randomizer.Serialized;

namespace Randomizer
{
    public static partial class RandoResources
    {
        [CompilerGenerated]
        public static Location[] Shops = new Location[]
        {
            new ObjectLocation("Sly", "Room_shop", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[0], new string[0], "Shop Menu"),
            new ObjectLocation("Sly_(Key)", "Room_shop", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[] { new PlayerField<bool>("gaveSlykey", true, false) }, new string[0], "Shop Menu"),
            new ObjectLocation("Iselda", "Room_mapper", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[0], new string[0], "Shop Menu"),
            new ObjectLocation("Salubra", "Room_Charm_Shop", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[0], new string[0], "Shop Menu"),
            new ObjectLocation("Leg_Eater", "Fungus2_26", new string[0], new string[0], new string[0], new PlayerField<int>[0], new PlayerField<bool>[0], new string[0], "Shop Menu")
        };
    }
}

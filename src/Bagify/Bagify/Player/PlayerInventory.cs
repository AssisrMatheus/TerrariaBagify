using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bagify.BagifyClasses.Player
{
    public enum InventoryType
    {
        Armor,
        Dye,
        Inventory
    }

    public class InventoryItem
    {
        public InventoryItem(int netId, int index, InventoryType type)
        {
            this.NetId = netId;
            this.Index = index;
            this.Type = type;
        }

        public int NetId { get; set; }
        public int Index { get; set; }

        public InventoryType Type { get; set; }
    }
}

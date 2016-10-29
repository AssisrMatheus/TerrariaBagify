using Bagify.BagifyClasses.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace Bagify
{
    public class Storage
    {
        public static IEnumerable<InventoryItem> InventoryCache { get; set; }
    }

    public class CommandNames
    {
        public readonly static string EquipItem = "BagifyEquipItem";
    }

    public class CommandHotkeys
    {
        public readonly static string EquipItem = "Q";
    }

	class Bagify : Mod
	{
        /// <summary>
        /// On game/mod started
        /// </summary>
		public Bagify()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}

        /// <summary>
        /// On mod loaded
        /// </summary>
        public override void Load()
        {
            base.Load();
            Storage.InventoryCache = new List<InventoryItem>();

            //Adds new global item for on item pickup listener
            AddGlobalItem("BagifyGlobalItem", new BagifyGlobalItem());

            //Register a hotkey for equiping
            RegisterHotKey(CommandNames.EquipItem, CommandHotkeys.EquipItem);
        }

        /// <summary>
        /// Default method to override hotkey press
        /// </summary>
        public override void HotKeyPressed(string name)
        {
            base.HotKeyPressed(name);

            //Invoke the current class method with the same name as the given hotkeypressed name
            this.GetType().GetMethod(name).Invoke(this, null);
        }

        /// <summary>
        /// Equip the selected item
        /// </summary>
        public void BagifyEquipItem()
        {
            if(Main.mouseItem.netID != 0)
            {
                ItemSlot.SwapEquip(ref Main.mouseItem);
                Main.mouseItem = new Item();
            }
            else
                ItemSlot.SwapEquip(Main.player[Main.myPlayer].inventory, 0, Main.player[Main.myPlayer].selectedItem);
        }
    }

    public class BagifyGlobalItem : GlobalItem
    {
        public override bool OnPickup(Item item, Player player)
        {
            //If it's not normal difficulty
            if (player.difficulty != 0)
            {
                var items = Storage.InventoryCache.ToList();

                //Tries to find an item from the last death with the same id
                var current = items.Where(x => x.NetId == item.netID).FirstOrDefault();
                
                if (current != null)
                {
                    //Saves the item in the current position
                    var existing = player.inventory[current.Index];

                    if (current.Type != InventoryType.Inventory)
                    {
                        //Gets the first empty slot
                        var emptyInd = player.inventory.ToList().IndexOf(player.inventory.Where(x => x.netID == 0).FirstOrDefault());
                        player.inventory[emptyInd] = item;
                        ItemSlot.SwapEquip(player.inventory, 0, emptyInd);

                        if (current.Type == InventoryType.Armor)
                            return !player.armor.Select(x => x.netID).Contains(item.netID);
                        else if (current.Type == InventoryType.Dye)
                            return !player.dye.Select(x => x.netID).Contains(item.netID);
                    }
                    else
                    {
                        //Replaces the item on inventory with the matched item
                        player.inventory[current.Index] = item;
                    }

                    //Gets the first empty slot
                    var emptyIndex = player.inventory.ToList().IndexOf(player.inventory.Where(x => x.netID == 0).FirstOrDefault());

                    //Place the previous item in any empty slot
                    player.inventory[emptyIndex] = existing;

                    //Remove from the list
                    items.Remove(current);
                    Storage.InventoryCache = items;
                }                

                //Return false here to prevent duplicate
                return current == null;
            }

            return true;
        }
    }
    public class BagifyModPlayer : ModPlayer
    {
        /// <summary>
        /// Just before the player is killed
        /// </summary>
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref string deathText)
        {
            //If it's not normal difficulty
            if (Main.player[Main.myPlayer].difficulty != 0)
            {
                var currentCachedInventory = Storage.InventoryCache.ToList();

                //Select every current using armor
                var armors = Main.player[Main.myPlayer].armor.Select(x =>
                {
                    var index = Main.player[Main.myPlayer].armor.ToList().IndexOf(x);
                    return new InventoryItem(x.netID, index, InventoryType.Armor);
                });

                currentCachedInventory.AddRange(armors);

                //Select every current using dye
                var dyes = Main.player[Main.myPlayer].dye.Select(x =>
                {
                    var index = Main.player[Main.myPlayer].dye.ToList().IndexOf(x);
                    return new InventoryItem(x.netID, index, InventoryType.Dye);
                });

                currentCachedInventory.AddRange(dyes);

                //Select every current item on inventory
                var inventory = Main.player[Main.myPlayer].inventory.Select(x =>
                {
                    var index = Main.player[Main.myPlayer].inventory.ToList().IndexOf(x);
                    return new InventoryItem(x.netID, index, InventoryType.Inventory);
                });

                currentCachedInventory.AddRange(inventory);

                //Recache
                Storage.InventoryCache = currentCachedInventory;
            }
            
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref deathText);
        }
    }
}

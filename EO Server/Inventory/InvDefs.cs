using System;
using System.Collections.Generic;
using EO_Server;

namespace EO_Server
{
    public enum ItemType : uint
    {
        GOLD,
        STATIC,
        POTION,
        HAT,
        ARMOR,
        WEAPON,
        NECKLACE,
        BACK,
        GLOVES,
        BELT,
        CHARM,
        BOOTS,
        RING,
        BRACELET,
        BRACER
    }
    public interface IInventory
    {
        public Entity Owner { get; }

    }

    public interface IInventoryItem
    {
        public IInventory Inv { get; set; }
        public uint ItemId { get; }
        public uint Quantity { get; set; }
        public ItemType Type { get; }
    }

    public class PlayerInventory : IInventory
    {
        private Character owner;

        public Entity Owner { get { return owner; } }
        public Dictionary<uint, PlayerInvItem> items;
        public int[,] slots;

        public static Vector2 SLOT_SIZE = new Vector2(14, 4);

        public PlayerInventory(Character player)
        {
            items = new Dictionary<uint, PlayerInvItem>();
            owner = player;

            //Init slots array
            slots = new int[SLOT_SIZE.x, SLOT_SIZE.y];

            for (int x = 0; x < SLOT_SIZE.x; x++)
            {
                for (int y = 0; y < SLOT_SIZE.y; y++)
                {
                    slots[x, y] = -1;
                }
            }

        }

        public PlayerInvItem AddItem(ItemEntity itemEnt)
        {
            return AddItem((uint) itemEnt.itemId, (uint) itemEnt.quantity);
        }

        //TODO: Change for args 'pos'
        public PlayerInvItem AddItem(uint itemId, uint quantity)
        {
            //Exists in inventory
            if (items.TryGetValue(itemId, out PlayerInvItem item))
            {
                item.Quantity += quantity;
                
                //item.Sync();

                return item;
            }
            else
            {
                Vector2? p = GetFreeSpace(itemId);

                if (p is Vector2 pos)
                {
                    item = new PlayerInvItem(this, itemId, quantity, pos);
                    items.Add(itemId, item);

                    //Update to player
                    if (owner != null)
                    {
                        SetPlayerInvItem packet = new SetPlayerInvItem(item.ItemId, item.Quantity, (uint)item.position.x, (uint)item.position.y);
                        owner.client.SendPacket(packet);
                    }
                   

                    return item;
                }
                else
                {
                    //NO SPACE!
                }
            }

            return null;
        }

        //Adds an item at certain position without checking if position contains another item
        public void AddItem(uint itemId, uint quantity, Vector2 position)
        {
            PlayerInvItem item = new PlayerInvItem(this, itemId, quantity, position);
            items.Add(itemId, item);

            if (owner != null)
            {
                SetPlayerInvItem packet = new SetPlayerInvItem(item.ItemId, item.Quantity, (uint)item.position.x, (uint)item.position.y);
                owner.client.SendPacket(packet);
            }
        }

        //TODO: Merge with 'SetItemAmount' method
        public bool ReduceItem(uint itemId, uint quantity)
        {
            PlayerInvItem item = GetItem(itemId);

            if (item != null)
            {
                ReduceItem(item, quantity);
                return true;
            }

            return false;
        }

        public void ReduceItem(PlayerInvItem item, uint quantity)
        {
            item.Quantity = Math.Max(item.Quantity - quantity, 0);
            /*
            if(item.Quantity == 0)
            {
                RemoveItem(item.ItemId);
            }

            item.Sync();
            */
        }

        public void RemoveItem(uint itemId)
        {
            if(items.Remove(itemId, out PlayerInvItem item))
            {
                item.ClearInvSpace();
            }
        }

        //Can the inventory hold this quantity of item? If so, how much (returns 0 -> quantity)
        public uint CanHoldItem(uint itemId, uint quantity)
        {
            if (items.TryGetValue(itemId, out PlayerInvItem item))
            {
                return quantity;
            }
            else
            {
                Vector2? p = GetFreeSpace(itemId);

                if (p is Vector2 pos)
                {

                    return quantity;
                }
                else
                {
                    //NO SPACE!
                }
            }

            return 0;
        }

        public bool IsInBounds(Vector2 pos)
        {
            if (pos.x >= 0 && pos.y >= 0 && pos.x < SLOT_SIZE.x && pos.y < SLOT_SIZE.y)
                return true;

            return false;
        }

        public bool IsInBounds(Vector2 pos, Vector2 size)
        {
            if (pos.x >= 0 && pos.y >= 0 && (pos.x + size.x) <= SLOT_SIZE.x && (pos.y + size.y) <= SLOT_SIZE.y)
                return true;

            return false;
        }

        public bool ContainsItem(Vector2 pos)
        {
            if (slots[pos.x, pos.y] >= 0)
                return true;

            return false;
        }


        public bool ContainsItem(Vector2 pos, Vector2 size)
        {
            int posEndX = pos.x + size.x - 1;
            int posEndY = pos.y + size.y - 1;

            //Clamp the values
            posEndX = (posEndX >= SLOT_SIZE.x) ? (SLOT_SIZE.x - 1) : posEndX;
            posEndY = (posEndY >= SLOT_SIZE.y) ? (SLOT_SIZE.y - 1) : posEndY;

            for (int x = pos.x; x <= posEndX; x++)
            {
                for (int y = pos.y; y <= posEndY; y++)
                {
                    if (slots[x, y] >= 0)
                        return true;
                }
            }



            return false;
        }

        //Ignores the space taken by item
        //Returns true if the space covered contains any item (except arg 'item') 
        public bool ContainsItem(PlayerInvItem item, Vector2 pos, Vector2 size)
        {
            int posEndX = pos.x + size.x - 1;
            int posEndY = pos.y + size.y - 1;

            //Clamp the values
            posEndX = (posEndX >= SLOT_SIZE.x) ? (SLOT_SIZE.x - 1) : posEndX;
            posEndY = (posEndY >= SLOT_SIZE.y) ? (SLOT_SIZE.y - 1) : posEndY;

            for (int x = pos.x; x <= posEndX; x++)
            {
                for (int y = pos.y; y <= posEndY; y++)
                {
                    //Occupied by an item other than the passed item argument
                    if (slots[x, y] >= 0 && slots[x, y] != item.ItemId)
                        return true;
                }
            }



            return false;
        }

        //TODO: Implement
        public void SwapItems(int itemId1, int itemId2)
        {
            throw new NotImplementedException();
        }

       
        public bool MoveItem(uint itemId, Vector2 pos)
        {
            if(items.TryGetValue(itemId, out PlayerInvItem item))
            {
                if(!ContainsItem(item, pos, item.size))
                {
                    item.SetSlotPosition(pos);
                    return true;
                }
            }


            return false;
        }

        //Assume we have that item
        public void SetItemAmount(uint itemId, uint quantity)
        {
            PlayerInvItem item = GetItem(itemId);
            if (item != null)
            {
                item.Quantity = quantity;
                /*
                if (quantity <= 0)
                {
                    item.Quantity = 0;
                    RemoveItem(itemId);
                }
                else
                {
                    item.Quantity = quantity;
                }
                item.Sync();
                */

            }
        }

        public Vector2? GetFreeSpace(uint itemId)
        {
            ItemDataEntry data = DataFiles.itemDataFile.entries[(int) itemId];

            for (int x = 0; x <= (SLOT_SIZE.x - data.sizeX); x++)
            {
                for (int y = 0; y <= (SLOT_SIZE.y - data.sizeY); y++)
                {
                    if (!ContainsItem(new Vector2(x, y), new Vector2((int)data.sizeX, (int)data.sizeY)))
                    {
                        return new Vector2(x, y);
                    }
                }
            }

            return null;
        }

        public PlayerInvItem GetItem(uint itemId)
        {
            items.TryGetValue(itemId, out PlayerInvItem item);

            return item;
        }

        public void Sync()
        {
            foreach(PlayerInvItem item in items.Values)
            {
                item.Sync();
            }
        }
    }

    public class ChestInventory : IInventory
    {
        private Chest owner;
        private List<ChestItem> items;

        public Entity Owner { get { return owner; } }
        public List<ChestItem> Items { get { return items; } }

        public ChestInventory(Chest owner)
        {
            this.owner = owner;
            this.items = new List<ChestItem>();
        }

        //Automatically adds a new chest item to the last slot index
        public void AddItem(uint itemId, uint quantity)
        {
            uint slotIndex = (uint) items.Count;
            ChestItem item = new ChestItem(this, itemId, quantity, slotIndex);
            items.Add(item);

            //Sync
            foreach(Character c in owner.listening)
            {
                item.Sync(c);
            }
        }

        //TODO: Handle slotIndex shifting
        public void RemoveItem(uint slotIndex)
        {
            items.RemoveAt((int)slotIndex);

            //Shift new indices!
            for(uint i = 0; i < items.Count; i++)
            {
                items[(int)i].slotIndex = i;
            }
        }
    }



    public class PlayerInvItem : IInventoryItem
    {
        private uint itemId;
        private uint quantity;
        private PlayerInventory inv;
        private ItemType type;

        public IInventory Inv { get { return inv; } set { inv = value as PlayerInventory; } }
        public uint ItemId { get { return itemId; } }
        public uint Quantity 
        { 
            get { return quantity; } 
            set 
            { 
                quantity = value;
                Sync();

                if(quantity == 0 && type != ItemType.GOLD)
                    inv.RemoveItem(itemId);
                
                    
            } 
        }
        public ItemType Type { get { return type;  } }

        public Vector2 position;
        public Vector2 size;

        public string name;

        

        public PlayerInvItem(PlayerInventory inv, uint itemId, uint quantity, Vector2 pos)
        {
            ItemDataEntry data = DataFiles.itemDataFile.entries[(int) itemId];
            this.inv = inv;
            this.itemId = itemId;
            this.quantity = quantity;
            this.size = new Vector2((int)data.sizeX, (int)data.sizeY);

            this.name = data.name;
            this.type = (ItemType) data.itemType;

            InitPosition(pos);
        }

        //Sync this item with player
        public void Sync()
        {
            if(inv != null && inv.Owner != null)
            {
                var character = inv.Owner as Character;
                SetPlayerInvItem defPacket = new SetPlayerInvItem(ItemId, Quantity, (uint)position.x, (uint)position.y);
                character.client.SendPacket(defPacket);
            }
        }

        private void InitPosition(Vector2 pos)
        {
            int posEndX = pos.x + size.x - 1;
            int posEndY = pos.y + size.y - 1;


            for (int x = pos.x; x <= posEndX; x++)
            {
                for (int y = pos.y; y <= posEndY; y++)
                {
                    inv.slots[x, y] = (int) ItemId;
                }
            }

            position = pos;
        }

        public void SetSlotPosition(Vector2 newPos)
        {
            ClearInvSpace();


            int posEndX = newPos.x + size.x - 1;
            int posEndY = newPos.y + size.y - 1;


            for (int x = newPos.x; x <= posEndX; x++)
            {
                for (int y = newPos.y; y <= posEndY; y++)
                {
                    inv.slots[x, y] = (int)ItemId;
                }
            }

            position = newPos;

            Sync();
        }
        //When quantity reaches 0 (item doesn't exist anymore), clear it from the slots array
        //Or when moved from one pos to another
        public void ClearInvSpace()
        {
            //Rempve from old position
            int posEndX = position.x + size.x - 1;
            int posEndY = position.y + size.y - 1;


            for (int x = position.x; x <= posEndX; x++)
            {
                for (int y = position.y; y <= posEndY; y++)
                {
                    inv.slots[x, y] = -1;
                }
            }
        }

    }

    public class ChestItem : IInventoryItem
    {
        private ChestInventory inv;
        private uint itemId;
        private uint quantity;
        private ItemType type;

        public uint slotIndex;


        public IInventory Inv { get { return inv; } set => throw new NotImplementedException(); }

        public uint ItemId { get { return itemId; } }

        public uint Quantity { get { return quantity; } set { quantity = value; } }

        public ItemType Type { get { return type; } }

        public ChestItem(ChestInventory inv, uint itemId, uint quantity, uint slotIndex)
        {
            this.inv = inv;
            this.itemId = itemId;
            this.quantity = quantity;
            this.slotIndex = slotIndex;
            ItemDataEntry data = DataFiles.GetItemData((int)itemId);
            this.type = (ItemType)data.itemType;
        }

        public void Sync(Character character)
        {
            SetChestInvItem packet = new SetChestInvItem(itemId, quantity, slotIndex);
            character.client.SendPacket(packet);
        }
    }


    public enum EquipResult
    {
        SUCCESS,
        SAME_EQUIP,
        NO_SPACE_INV
    }
}

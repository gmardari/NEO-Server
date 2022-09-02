using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public class Chest : Entity
    {
        public ChestInventory inv;
        public List<Character> listening;

        public Chest(EOMap map, Vector2 pos) : base(map, pos)
        {
            this.inv = new ChestInventory(this);
            this.entityType = EntityType.CHEST;
            this.listening = new List<Character>();
        }

        public void OpenChest(Character character)
        {
            if (inv != null)
            {
                listening.Add(character);
                character.chest_open = this;
                ChestOpen packet = new ChestOpen(this.entityId);
                character.client.SendPacket(packet);

                foreach(ChestItem item in inv.Items)
                {
                    item.Sync(character);
                }

                Ack ack = new Ack((byte)RESOURCE_TYPE.CHEST_CONTENTS);
                character.client.SendPacket(ack);
            }
        }

        public void StopListening(Character character)
        {
            if(listening.Remove(character))
            {
                character.chest_open = null;
            }
        }

        public uint TakeItem(Character character, uint slotIndex, uint quantity)
        {
            if(!listening.Contains(character))
                return 0;

            if(slotIndex < inv.Items.Count)
            {
                ChestItem item = inv.Items[(int)slotIndex];
                //We can't take more quantity than there already is
                quantity = Math.Min(quantity, item.Quantity);

                uint num_hold = character.inv.CanHoldItem(item.ItemId, quantity);

                if(num_hold > 0)
                {
                    character.inv.AddItem(item.ItemId, num_hold);
                    item.Quantity -= num_hold;

                    //Remove chest item
                    if (item.Quantity == 0)
                    {
                        inv.RemoveItem(item.slotIndex);
                    }
                    
                    foreach(Character c in listening)
                    {
                        item.Sync(c);
                    }

                    return num_hold;
                }
            }

            return 0;
        }

        //TODO: Check if item is untradable/unstorable
        public bool GiveItem(Character character, uint itemId, uint r_quant)
        {
            PlayerInvItem playerItem = character.inv.GetItem(itemId);
            if(playerItem != null)
            {
                //Can't give more than what player already has!
                uint quantity = Math.Min(r_quant, playerItem.Quantity);
                inv.AddItem(itemId, quantity);
                playerItem.Quantity -= quantity;

            }
          
            return false;
        }

        public override void NetInitAll()
        {
            SetEntityDef defPacket = new SetEntityDef(this.entityId, (uint)this.entityType, position.x, position.y);
            map.SendPacketToClients(defPacket);
        }

        public override void NetInit(NetworkClient client) 
        {
            SetEntityDef defPacket = new SetEntityDef(this.entityId, (uint) this.entityType, position.x, position.y);
            client.SendPacket(defPacket);
        }
    }
}

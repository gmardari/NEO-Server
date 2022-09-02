using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public class ItemEntity : Entity
    {
        public int itemId;
        public int quantity;

        public ItemEntity(EOMap map, int itemId, int quantity, Vector2 pos) : base(map, pos)
        {
            (this.map, this.itemId, this.quantity) = (map, itemId, quantity);
            entityType = EntityType.ITEM;
        }
    }
}

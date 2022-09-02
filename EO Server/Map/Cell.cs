using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public class Cell
    {
        //0 is nothing (black)
        public int groundLayerId;
        public int specialLayerId;
        public LinkedList<Entity> entities;

        public Cell(int groundLayerId, int specialLayerId)
        {
            this.groundLayerId = groundLayerId;
            this.specialLayerId = specialLayerId;
            this.entities = new LinkedList<Entity>();
        }

        public void AddEntity(Entity entity)
        {
            entities.AddLast(entity);
        }

        public Entity GetEntityOfType<T>()
        {
            foreach(Entity entity in entities)
            {
                if(entity.GetType().Equals(typeof(T)))
                {
                    return entity;
                }
            }

            return null;
        }

        public bool RemoveEntity(Entity entity)
        {
            return entities.Remove(entity);
        }

        public bool RemoveEntityFromId(ulong entityId)
        {
            for (var node = entities.First; node != null; node = node.Next)
            {
                if (node.Value.entityId == entityId)
                {
                    entities.Remove(node);
                    return true;
                }
            }

            return false;
        }

        public bool HasEntity(Entity entity)
        {
            return entities.Contains(entity);
        }
    }
}

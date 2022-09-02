using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{

    public class Entity
    {
        public EOMap map;
        public EntityType entityType;
        public ulong entityId;
        public Vector2 position;

        public bool valid; //Valid signifies whether the entity is alive and functioning on a map

        public Entity(EOMap map, Vector2 position)
        {
            this.map = map;
            this.position = position;
            this.entityId = SpawnManager.GetAvailableEntityId();
            valid = true;
        }

        public virtual void Update()
        {

        }

        public virtual void SetMap(EOMap map)
        {
            this.map = map;
        }

        public virtual void SetPosition(Vector2 newPos, bool net_sync)
        {
            if (newPos != position)
            {
                Cell oldTile = map.GetCell(position);
                Cell newTile = map.GetCell(newPos);

                oldTile.RemoveEntity(this);
                newTile.AddEntity(this);
            }

            position = newPos;

            if(net_sync)
                map.SendPacketToClients(new SetEntityPos(entityId, position.x, position.y));
        }

        //Initializes the entity to all clients on the map
        public virtual void NetInitAll()
        {

        }

        //Initializes the entity to the client for the first time
        public virtual void NetInit(NetworkClient client)
        {

        }

        public virtual void Sync()
        {

        }

        public virtual void Destroy()
        {
            map.RemoveEntity(this);
        }
    }




}

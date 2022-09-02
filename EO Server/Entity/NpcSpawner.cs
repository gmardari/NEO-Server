using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public class NpcSpawner : Entity
    {
        public int npcId;
        public IntRange respawnTime;
        public IntRange fidgetTime;
        public long respawnInterval;

        //The current NPC instance that was last spawned from this instance
        public ulong? npcEntityId;
        public long? nt_death;
        public bool flag_spawn;
        private bool invalidNpcId;

        public NpcSpawner(EOMap map, int npcId, Vector2 _pos, IntRange respawnTime, IntRange fidgetTime) : base(map, _pos)
        {
            this.entityType = EntityType.NPC_SPAWNER;
            this.npcId = npcId;
            this.respawnTime = respawnTime;
            this.fidgetTime = fidgetTime;
            this.position = _pos;
            this.flag_spawn = true;
            map.OnEntityRemoved += OnEntityRemoved;
        }

        //TODO: Randomize direction, add random pos spawning
        public bool Spawn()
        {
            uint dir = 0;
            return map.SpawnNpc(npcId, position, dir, fidgetTime, out npcEntityId);
        }

        public void OnEntityRemoved(Entity entity)
        {
            if(npcEntityId != null)
            {
                if(npcEntityId == entity.entityId)
                {
                    Console.WriteLine("NPC Spawner detected NPC death. Death timer will be set.");
                    nt_death = Server.GetCurrentTime();
                    Random rand = new Random();
                    respawnInterval = rand.Next(respawnTime.min, respawnTime.max + 1) * 1000;
                }
            }
        }

        public override void Update()
        {
            if (invalidNpcId)
                return;

            if (nt_death != null && (Server.GetCurrentTime() - nt_death) > respawnInterval)
            {
                flag_spawn = true;
                nt_death = null;
            }

            if (flag_spawn)
            {
                Console.WriteLine("Spawning on NpcSpawner");

                if (Spawn())
                {
                    flag_spawn = false;
                }
                else
                {
                    invalidNpcId = true;
                    Console.Error.WriteLine($"Could not spawn Npc Id {npcId}");
                }
            }

            
        }
    }
}

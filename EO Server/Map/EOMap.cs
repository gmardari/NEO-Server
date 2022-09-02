using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{

    public struct MapBounds
    {
        public Vector2 minPoint;
        public Vector2 maxPoint;
        public Vector2 size;

        public MapBounds(Vector2 _minPoint, Vector2 _size)
        {
            (minPoint, size) = (_minPoint, _size);
            maxPoint = minPoint + new Vector2(size.x - 1, size.y - 1);
        }
    }

    public class EOMap
    {
        public static uint spawnMapId = 1;
        public static Vector2 spawnPoint = new Vector2(0, 0);

        public uint mapId;
        public string mapName;
        public MapBounds bounds;
        public Cell[,] cells;
        private Dictionary<ulong, Entity> entities;
        private Dictionary<ulong, NetworkClient> clients;
        private List<Vector2> cell_walls;
        private LinkedList<NpcSpawner> npc_spawners;
        public bool isLoaded = default;

        private Queue<NetworkClient> unconnectedClients;
        private Queue<Entity> _addEntities;
        private Queue<Entity> _removeEntities;

        public delegate void EntityRemovedEvent(Entity entity);
        public delegate void EntityAddedEvent(Entity entity);
        public EntityRemovedEvent OnEntityRemoved;
        public EntityAddedEvent OnEntityAdded;

        //Assumes container is verified 
        public EOMap(MapContainer container)
        {
            mapId = (uint) container.mapId;
            mapName = container.mapName;
            bounds = new MapBounds(new Vector2(container.minX, container.minY), new Vector2(container.width, container.height));
            cells = new Cell[container.width, container.height];
            cell_walls = new List<Vector2>();
            npc_spawners = new LinkedList<NpcSpawner>();
            entities = new Dictionary<ulong, Entity>();
            clients = new Dictionary<ulong, NetworkClient>();
            unconnectedClients = new Queue<NetworkClient>();
            _addEntities = new Queue<Entity>();
            _removeEntities = new Queue<Entity>();
            //TODO: Fix and re-enable
            
            int i, npcIndex, warpIndex, chestIndex;
            i = npcIndex = warpIndex = chestIndex = 0;

            for(int x = bounds.minPoint.x; x <= bounds.maxPoint.x; x++)
            {
                for (int y = bounds.minPoint.y; y <= bounds.maxPoint.y; y++)
                {
                    int xShifted = x - bounds.minPoint.x;
                    int yShifted = y - bounds.minPoint.y;

                    
                    int groundTileId = container.groundLayer[i];
                    int specTileId = container.specialLayer.tiles[i];

                    bool wallAdded = false;

                    cells[xShifted, yShifted] = new Cell(groundTileId, specTileId);

                    //Wall! Add it to the entry
                    if (groundTileId < 0)
                    {
                        cell_walls.Add(new Vector2(x, y));
                        wallAdded = true;
                        //Console.WriteLine($"Wall at {x}, {y}");
                    }

                    
                    //Load special tiles
                    switch (specTileId)
                    {
                        case (int)MapSpecialIndex.WALL:
                            {
                                if (!wallAdded)
                                {
                                    cell_walls.Add(new Vector2(x, y));
                                    wallAdded = true;
                                }
                                break;
                            }
                        case (int)MapSpecialIndex.NPC_SPAWN:
                            {
                                var spawnInfo = container.specialLayer.npcSpawnList[npcIndex++];
                                //DataFiles.npcDataFile.Entries.ContainsKey(spawnInfo.npcId)

                                //Npc ID is not out of range
                                if (DataFiles.npcDataFile.Entries.Count > spawnInfo.npcId)
                                {
                                    NpcSpawner spawner = new NpcSpawner(this, spawnInfo.npcId,
                                        new Vector2(x, y), new IntRange(spawnInfo.respawnTimeMin, spawnInfo.respawnTimeMax),
                                        new IntRange(spawnInfo.fidgetTimeMin, spawnInfo.fidgetTimeMax));

                                    AddEntity(spawner);
                                    npc_spawners.AddLast(spawner);
                                    Console.WriteLine("Added spawner");
                                }

                                break;
                            }
                        case (int)MapSpecialIndex.WARP:
                            {
                                var warpInfo = container.specialLayer.warpList[warpIndex++];

                                Warp warp = new Warp(this, new Vector2(x, y), (uint)warpInfo.mapId,
                                    new Vector2(warpInfo.x, warpInfo.y), (uint)warpInfo.direction);
                                AddEntity(warp);


                                break;
                            }
                        case (int)MapSpecialIndex.CHEST:
                            {
                                var chestInfo = container.specialLayer.chestList[chestIndex++];

                                Chest chest = new Chest(this, new Vector2(x, y));

                               
                                foreach (var spawnInfo in chestInfo.spawnList)
                                {
                                    Random random = new Random();
                                    uint quantity = 0;

                                    if (spawnInfo.quantityRange.min == spawnInfo.quantityRange.max)
                                        quantity = (uint) spawnInfo.quantityRange.min;
                                    else
                                        quantity = (uint) random.Next(spawnInfo.quantityRange.min, spawnInfo.quantityRange.max + 1); 

                                    chest.inv.AddItem(spawnInfo.itemId, quantity);
                                }

                                AddEntity(chest);

                                if (!wallAdded)
                                {
                                    cell_walls.Add(new Vector2(x, y));
                                    wallAdded = true;
                                }

                                break;
                            }
                    }
                    


                    i++;

                    if(!wallAdded)
                        AddEntity(new ItemEntity(this, i%5, 15, new Vector2(x, y)));

                }
            } //FINISH iterating through tiles


            AddEntity(new NpcSpawner(this, 0, new Vector2(1, 1), new IntRange(5, 6), new IntRange(5, 10)));


            isLoaded = true;
        }

        public bool OutOfMapBounds(Vector2 pos)
        {
            if (pos.x < bounds.minPoint.x || pos.x > bounds.maxPoint.x || pos.y < bounds.minPoint.y || pos.y > bounds.maxPoint.y)
                return true;

            return false;
        }

        public bool CanMoveToPos(Vector2 pos)
        {
            if (OutOfMapBounds(pos))
                return false;

            if (cell_walls.Contains(pos))
                return false;


            return true;
        }

        public bool CanMoveToPos(Vector2 from, uint direction)
        {
            Vector2 newPos = PositionInDirection(from, direction);

            if (OutOfMapBounds(newPos))
                return false;

            if (cell_walls.Contains(newPos))
                return false;


            return true;
        }

        //TODO: Update!!!
        public void AddEntity(Entity entity)
        {
            entities.Add(entity.entityId, entity);
            Cell cell = GetCell(entity.position);
            cell.AddEntity(entity);

            //Update clients
            //SetEntityDef defPacket = new SetEntityDef(entity.entityId, (uint)entity.entityType, entity.position.x, entity.position.y);

            //SendPacketToClients(defPacket);

            if (entity is Character)
            {
                var character = (Character)entity;

                SetCharacterDef defPacket = new SetCharacterDef(entity.entityId, entity.position.x, entity.position.y,
                    character.direction, character.props.health, character.props.maxHealth, character.def);

                SendPacketToClients(defPacket);
            }
            else if (entity is Mob)
            {
                var mob = (Mob)entity;

                //SetEntityDir dirPacket = new SetEntityDir(mob.entityId, mob.direction);
                SetNpcDef npcDefPacket = new SetNpcDef(mob.entityId, entity.position.x, entity.position.y, mob.direction,
                    mob.health, mob.maxHealth, (uint)mob.def.npcId);

                //SendPacketToClients(dirPacket);
                SendPacketToClients(npcDefPacket);
            }
            else if (entity is ItemEntity)
            {
                ItemEntity item = entity as ItemEntity;

                SetItemDef defPacket = new SetItemDef(item.entityId, item.position.x, item.position.y, (uint)item.itemId, (uint)item.quantity);
                SendPacketToClients(defPacket);
            }
            else
                entity.NetInitAll();
        }

        public void RemoveEntity(Entity entity)
        {
            entity.valid = false;
            entities.Remove(entity.entityId);
            Cell tile = GetCell(entity.position);
            tile.RemoveEntity(entity);

            var packet = new RemoveEntity(entity.entityId);

            if(entity.entityType != EntityType.PLAYER)
                SendPacketToClients(packet);
            else
            {
                foreach(var client in clients.Values)
                {
                    //Do not remove the local player for the client
                    if (client.character.entityId == entity.entityId)
                        continue;

                    client.SendPacket(packet);
                }
            }

            if (OnEntityRemoved != null)
                OnEntityRemoved(entity);
        }

        public Character SpawnPlayer(NetworkClient _client, CharacterDef def, string invString, CharProperties props, Vector2 pos, uint direction)
        {
            Character character = new Character(_client, this, def, props, pos, direction);

            if(invString != null)
                UtilFunctions.DeserializePlayerInv(character, invString);
            

            //Will update all other clients about this new character
            AddEntity(character);
            
            //Updates client on all entities including client's character
            ClientInitMap(_client, character, true);
            //Sync player inventory
            character.inv.Sync();

            //Client must be added AFTER adding the entity
            clients.Add(_client.clientId, _client);

            return character;
        }

        public void WarpPlayer(Character character, Vector2 pos, uint dir)
        {
            var client = character.client;

            //Set map and pos and dir without syncing to clients AddEntity() needs to be called beforehand
            character.SetMap(this);
            character.SetPosition(pos, false);
            character.SetDirection(dir, false);

            AddEntity(character);

            ClientInitMap(client, character, false);

            //Update player's locaiton only to the player
            SetEntityPos posPacket = new SetEntityPos(character.entityId, character.position.x, character.position.y);
            SetEntityDir dirPacket = new SetEntityDir(character.entityId, character.direction);
            client.SendPacket(posPacket);
            client.SendPacket(dirPacket);

            //Sync player inventory
            character.inv.Sync();

            clients.Add(client.clientId, client);
        }

        //Updates -> client
        public void ClientInitMap(NetworkClient client, Character character, bool updateLocalPlayer)
        {
            ulong characterId = character.entityId;
            SetMapPacket mapPacket = new SetMapPacket(mapId, characterId);
            Server.listener.SendPacket(client, mapPacket);

            foreach(var entity in entities.Values)
            {
                if (entity.entityType == (uint)EntityType.PLAYER)
                {
                    //Don't update the local player
                    if (!updateLocalPlayer && entity.entityId == character.entityId)
                        continue;

                    Character entChar = entity as Character;

                    SetCharacterDef defPacket = new SetCharacterDef(entity.entityId, entity.position.x, entity.position.y,
                        entChar.direction, entChar.props.health, entChar.props.maxHealth, entChar.def);

                    client.SendPacket(defPacket);
                }
                else if (entity.entityType == EntityType.NPC)
                {
                    Mob mob = entity as Mob;
                    SetNpcDef npcDefPacket = new SetNpcDef(mob.entityId, mob.position.x, mob.position.y, mob.direction,
                        mob.health, mob.maxHealth, (uint)mob.def.npcId);

                    client.SendPacket(npcDefPacket);
                }
                else if (entity.entityType == EntityType.ITEM)
                {
                    ItemEntity item = entity as ItemEntity;

                    SetItemDef defPacket = new SetItemDef(item.entityId, item.position.x, item.position.y, (uint)item.itemId, (uint)item.quantity);
                    client.SendPacket(defPacket);
                }
                else
                    entity.NetInit(client);
            }

            client.OnMapLoaded();
            //Tells client that we finished transferring data about the map
            Server.listener.SendPacket(client, new Ack((byte) RESOURCE_TYPE.MAP));
        }

        //TODO: Set npcId to uint?
        public bool SpawnNpc(int npcId, Vector2 pos, uint dir, IntRange fidgetTime, out ulong? entityId)
        {
            if (npcId < DataFiles.npcDataFile.Entries.Count)
            {
                NpcDataEntry entry = DataFiles.npcDataFile.Entries[npcId];
                
                Mob mob = new Mob(this, new NpcDef(npcId, entry), pos, dir, fidgetTime);

                _addEntities.Enqueue(mob);
                entityId = mob.entityId;

                Console.WriteLine($"Spawned {mob.def.name} at pos {mob.position}");
                return true;
            }

            entityId = null;
            return false;
        }

        
        //client character is valid
        public void OnPlayerRequestDirection(NetworkClient client, int direction, bool walking)
        {
            if (direction > 3)
                return;

            Character character = client.character;
            var inputManager = client.inputManager;

            if (character.state == CharacterState.IDLE)
            {
                if (!character.CanMove)
                    return;

                //Initiate a walk sequence after being idle
                if(walking || inputManager.inputTimer >= 250)
                {
                    //If key is released, proceed with last used direction. Otherwise, exit
                    if (direction < 0)
                    {
                        if (inputManager.lastDirection >= 0)
                            direction = inputManager.lastDirection;
                        else
                            return;
                    }
                        
                    Vector2 newPos = EOMap.PositionInDirection(character.position, (uint) direction);

                    //TODO: Implement no-clip for admins
                    if(!OutOfMapBounds(newPos))
                    {
                        if (CanMoveToPos(newPos))
                        {
                            character.WalkTo((uint)direction);
                        }
                        else
                        {
                            Chest chest = (Chest) GetCell(newPos).GetEntityOfType<Chest>();

                            if (chest != null)
                            {
                                Console.WriteLine("Interact with chest");
                                chest.OpenChest(character);
                            }
                        }
                    }
                }
                else
                {
                    if(direction >= 0)
                    {
                        character.SetDirection((uint) direction, true);
                    }
                    
                }
            }
        }

        public void OnPlayerRequestAttack(NetworkClient client)
        {
            Character character = client.character;

            if(character.state == CharacterState.IDLE)
            {
                character.Attack();
                
                //Damage entity in direction of player

            }
        }

        public void OnPlayerReqPickupItem(NetworkClient client, ulong entityId)
        {
            Character character = client.character;

            if(entities.TryGetValue(entityId, out Entity entity))
            {
                if(entity is ItemEntity item)
                {
                    int distance = UtilFunctions.GetDistance(character.position, item.position);

                    if(distance <= 2)
                    {
                        PickupItem(character, item);
                    }
                }
            }
        }

        public void OnPlayerReqMoveItem(NetworkClient client, RequestItemMove packet)
        {
            Character character = client.character;
            
            character.inv.MoveItem(packet.itemId, new Vector2((int)packet.x, (int)packet.y));
        }

        public void OnPlayerReqEquip(NetworkClient client, RequestItemEquip packet)
        {
            Character character = client.character;
            byte slotIndex = packet.slotIndex;

            if (packet.equip)
            {
                //Player had the item
                var invItem = character.inv.GetItem(packet.itemId);

                if(invItem != null)
                {
                    
                    EquipResult equipResult = EquipItem(character, (PaperdollSlot)slotIndex, invItem);
                    Console.WriteLine($"[EQUIP] {equipResult}");
                    //character.inv.SetItemAmount(item.ItemId, item.Quantity - 1);
                    /*
                    if(invItem.Type == ItemType.ARMOR)
                    {
                        if(character.def.doll.armor > 0)
                        {
                            uint dollItemId = (character.def.doll.armor - 1);
                            //Trying to equip the same item that is currently equipped
                            if (dollItemId == invItem.ItemId)
                                return;

                            PlayerInvItem dollToInvItem = character.inv.AddItem(dollItemId, 1);

                            if(dollToInvItem != null)
                            {
                                character.inv.SetItemAmount(invItem.ItemId, invItem.Quantity - 1);
                                character.SetPaperdoll(PaperdollSlot.ARMOR, packet.itemId + 1);
                            }
                            else
                                Console.WriteLine("[PLAYER EQUIP] There was no space in your inventory");
                        }
                        //Free paperdoll slot
                        else
                        {
                            character.inv.SetItemAmount(invItem.ItemId, invItem.Quantity - 1);
                            character.SetPaperdoll(PaperdollSlot.ARMOR, packet.itemId + 1);
                        }
                    }
                    */
                }
            }
            else
            {
                UnequipItem(character, (PaperdollSlot)slotIndex);
            }
        }

        public void OnPlayerReqChestClose(NetworkClient client, ReqChestClose packet)
        {
            if (client.character.chest_open != null)
                client.character.chest_open.StopListening(client.character);
        }

        public void OnPlayerReqChestTake(NetworkClient client, ReqChestItemTake packet)
        {
            if(client.character.chest_open != null)
            {
                Chest chest = client.character.chest_open;
                chest.TakeItem(client.character, packet.slotIndex, packet.quantity);
            }
        }

        public void OnPlayerReqChestGive(NetworkClient client, ReqChestItemGive packet)
        {
            if (client.character.chest_open != null)
            {
                Chest chest = client.character.chest_open;
                chest.GiveItem(client.character, packet.itemId, packet.quantity);
            }
        }

        public EquipResult EquipItem(Character character, PaperdollSlot slot, PlayerInvItem item)
        {
            uint equippedId = character.def.doll.GetItemId(slot);

            if(equippedId > 0)
            {
                //Minus one since it's shifted by one
                equippedId--;

                if (equippedId == item.ItemId)
                    return EquipResult.SAME_EQUIP;

                PlayerInvItem dollToInvItem = character.inv.AddItem(equippedId, 1);

                if (dollToInvItem != null)
                {
                    character.inv.SetItemAmount(item.ItemId, item.Quantity - 1);
                    character.SetPaperdoll(slot, item.ItemId + 1);
                }
                else
                    return EquipResult.NO_SPACE_INV;
            }
            else
            {
                character.inv.SetItemAmount(item.ItemId, item.Quantity - 1);
                character.SetPaperdoll(slot, item.ItemId + 1);
            }

            return EquipResult.SUCCESS;
        }

        public bool UnequipItem(Character character, PaperdollSlot slot)
        {
            Paperdoll doll = character.def.doll;
            uint equippedId = character.def.doll.GetItemId(slot);

            if (equippedId > 0)
            {
                //Minus one since it's shifted by one
                equippedId--;

                PlayerInvItem dollToInvItem = character.inv.AddItem(equippedId, 1);
                //character.SetPaperdoll(PaperdollSlot.ARMOR, 0);

                if (dollToInvItem != null)
                {
                    character.SetPaperdoll(slot, 0);
                   
                    return true;
                }
                else
                    return false;
            }

            return false;
        }

        public void PickupItem(Character character, ItemEntity item)
        {
            Vector2? pos = character.inv.GetFreeSpace((uint) item.itemId);
            if (pos is Vector2 posVal)
            {
                character.inv.AddItem(item);
                item.Destroy();
            }
        }

        //Called from listener 'thread'
        public void OnLostClient(NetworkClient client)
        {
            //unconnectedClients.Enqueue(client);
            RemoveClient(client);
        }

        private void RemoveClient(NetworkClient client)
        {
            RemoveEntity packet = null;

            if (client.character != null)
            {
                client.character.map = null;
                RemoveEntity(client.character);

                packet = new RemoveEntity(client.character.entityId);
                
            }

            clients.Remove(client.clientId);

            if(packet != null)
            {
                SendPacketToClients(packet);
            }
        }

        
        public void SendPacketToClients(Packet packet)
        {
            Server.listener.SendClients(clients.Values, packet);
        }

        public void Update()
        {
            while (_removeEntities.Count > 0)
            {
                var entity = _removeEntities.Dequeue();
                RemoveEntity(entity);
            }

            while (_addEntities.Count > 0)
            {
                var entity = _addEntities.Dequeue();
                AddEntity(entity);
            }

            foreach(var entity in entities.Values)
            {
                entity.Update();
            }

        }

        public static Vector2 PositionInDirection(Vector2 current, uint direction)
        {
            if (direction == 0)
                return current + new Vector2(-1, 0);
            if (direction == 1)
                return current + new Vector2(0, -1);
            if (direction == 2)
                return current + new Vector2(1, 0);
            if (direction == 3)
                return current + new Vector2(0, 1);

            return current;
        }
        public Cell GetCell(Vector2 position)
        {
            if (OutOfMapBounds(position))
                return null;

            int xShifted = position.x - bounds.minPoint.x;
            int yShifted = position.y - bounds.minPoint.y;

            return cells[xShifted, yShifted];
        }

        public override string ToString()
        {
            return $"Map id: {mapId}, width: {bounds.size.x}, height: {bounds.size.y}, origin-x: {bounds.minPoint.x}, origin-y: {bounds.minPoint.y}";
        }
    }
}

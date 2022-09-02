using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public class StopWatch
    {
        public long timeStart;
        public long timeInterval;

        public void Start(long _timeInterval)
        {
            timeStart = Server.GetCurrentTime();
            timeInterval = _timeInterval;
        }

        public bool Check()
        {
            if ((Server.GetCurrentTime() - timeStart) >= timeInterval)
                return true;

            return false;
        }

        public void Restart()
        {
            timeStart = Server.GetCurrentTime();
        }
    }

    public class PlayerInputManager
    {
        public long lastRecInput;
        public long inputTimer;
        public int direction;
        public int lastDirection;

        public PlayerInputManager()
        {
            lastRecInput = -1;
            direction = -1;
            lastDirection = -1;
            inputTimer = 0L;
        }

        public void ClearInput()
        {
            lastDirection = -1;
            direction = -1;
            inputTimer = 0L;
        }

        public void SetInput(int _direction, long time)
        {
            lastDirection = direction;
            direction = _direction;

            long deltaTime = time - lastRecInput;

            if (direction >= 0)
                lastRecInput = time;

            if (lastDirection == direction)
                inputTimer += deltaTime;
            else
                inputTimer = 0L;
        }

    }

    public static class UtilFunctions
    {
        public static int GetDistance(Vector2 a, Vector2 b)
        {
            return (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y));
        }

        //TODO: Make it work for distances between a and b greater than 1
        public static uint GetDirection(Vector2 a, Vector2 b)
        {
            int deltaX = b.x - a.x;
            int deltaY = b.y - a.y;

            if (deltaX > 0)
                return 2;
            else if (deltaX < 0)
                return 0;

            if (deltaY > 0)
                return 3;
            else if (deltaY < 0)
                return 1;

            return 0;
        }

        public static string SerializePlayerInv(PlayerInventory inv)
        {
            if (inv.items.Count == 0)
                return null;

           StringBuilder sb = new StringBuilder();

            foreach(var item in inv.items.Values)
            {
                sb.Append(item.ItemId);
                sb.Append(',');
                sb.Append(item.Quantity);
                sb.Append(',');
                sb.Append(item.position.x);
                sb.Append(',');
                sb.Append(item.position.y);
                sb.Append(';');
            }

            return sb.ToString();
        }

        public static void DeserializePlayerInv(Character character, string invString)
        {
            string[] itemStrings = invString.Split(';');
            PlayerInventory inv = character.inv;

            foreach(var itemString in itemStrings)
            {
                if(!String.IsNullOrWhiteSpace(itemString))
                {
                    string[] parts = itemString.Split(',');
                    uint itemId = uint.Parse(parts[0]);
                    uint quantity = uint.Parse(parts[1]);
                    int posX = int.Parse(parts[2]);
                    int posY = int.Parse(parts[3]);

                    Console.WriteLine($"{itemId} {quantity} {new Vector2(posX, posY)}");

                    inv.AddItem(itemId, quantity, new Vector2(posX, posY));
                }
            }
        }

        public static string SerializePlayerPaperdoll(Paperdoll doll)
        {
            StringBuilder sb = new StringBuilder();

            /*
             *  HAT,
                ARMOR,
                WEAPON,
                NECKLACE,
                BACK,
                GLOVES,
                BELT,
                CHARM,
                BOOTS,
                RING_1,
                RING_2,
                BRACELET_1,
                BRACELET_2,
                BRACER_1,
                BRACER_2
             */

            sb.Append(doll.hat);
            sb.Append(',');
            sb.Append(doll.armor);
            sb.Append(',');
            sb.Append(doll.weapon);
            sb.Append(',');
            sb.Append(doll.necklace);
            sb.Append(',');
            sb.Append(doll.back);
            sb.Append(',');
            sb.Append(doll.gloves);
            sb.Append(',');
            sb.Append(doll.belt);
            sb.Append(',');
            sb.Append(doll.charm);
            sb.Append(',');
            sb.Append(doll.boots);
            sb.Append(',');
            sb.Append(doll.ring1);
            sb.Append(',');
            sb.Append(doll.ring2);
            sb.Append(',');
            sb.Append(doll.bracelet1);
            sb.Append(',');
            sb.Append(doll.bracelet2);
            sb.Append(',');
            sb.Append(doll.bracer1);
            sb.Append(',');
            sb.Append(doll.bracer2);

            return sb.ToString();
        }

        public static void DeserializePaperdoll(CharacterDef def, string paperdollString)
        {
            string[] parts = paperdollString.Split(',');

            for(int i = 0; i < parts.Length; i++)
            {
                def.doll.Set((PaperdollSlot)i, uint.Parse(parts[i]));
            }
        }
    }
}

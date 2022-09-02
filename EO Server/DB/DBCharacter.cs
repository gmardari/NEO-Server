using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    //All the fields of a character from the database, exlcuding the appearance. Used to spawn the characters for the first time
    public class DBCharacter
    {
        public long? health;
        public long? mana;
        public int? map;
        public int? x;
        public int? y;
        public int? direction;
        public string inventory;
        public string paperdoll;
    }
}

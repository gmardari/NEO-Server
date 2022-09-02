using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Data;

namespace EO_Server
{
    public class DB
    {
        public static SqliteConnection conn;

        public static bool Connected
        { get 
            { 
                if(conn != null)
                    return conn.State == ConnectionState.Open;
              

                return false;
            } }

        public static void Connect(string dbName)
        {
            conn = new SqliteConnection($"Data Source={dbName}.db");
            conn.Open();

            /*
            var command = conn.CreateCommand();

            command.CommandText = "";

            using(var reader = command.ExecuteReader())
            {
                while(reader.Read())
                {
                    
                }
            }
            */
        }

        public static bool Authenticate(string username, string password)
        {
            if (conn.State == ConnectionState.Open)
            {
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"SELECT * FROM accounts WHERE username = $username AND password = $password";
                cmd.Parameters.AddWithValue("$username", username);
                cmd.Parameters.AddWithValue("$password", password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    { 
                        Console.WriteLine($"Loaded account");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Wrong credentials");
                        return false;
                    }
                }
            }


            return false;
        }

        public static bool UsernameExists(string username)
        {
            if (conn.State == ConnectionState.Open)
            {
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"SELECT username FROM accounts WHERE username = $username";
                cmd.Parameters.AddWithValue("$username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }


            throw new Exception("Database closed");
        }

        public static bool CharacterExists(string charName)
        {
            if (conn.State == ConnectionState.Open)
            {
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"SELECT char_name FROM characters WHERE char_name = $charName";
                cmd.Parameters.AddWithValue("$charName", charName);
                
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }


            throw new Exception("Database closed");
        }

        public static bool CreateAccount(string username, string password)
        {
            if (conn.State == ConnectionState.Open)
            {
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"INSERT INTO accounts VALUES ($username, $password);";
                cmd.Parameters.AddWithValue("$username", username);
                cmd.Parameters.AddWithValue("$password", password);
                int numRowsInserted = cmd.ExecuteNonQuery();

                return (numRowsInserted > 0);
                
            }

            throw new Exception("Database closed");
        }

        public static bool CreateCharacter(string username, int charIndex, string charName, byte gender, byte skinColour, byte race, byte hairStyle, byte hairColour, out CharacterDef charDef)
        {
            if (conn.State == ConnectionState.Open)
            {
                var cmd = conn.CreateCommand();

                //Give the player the gold item using a fake inventory to serialize into a string
                PlayerInventory fakeInv = new PlayerInventory(null);
                fakeInv.AddItem(0, 0);
                string invString = UtilFunctions.SerializePlayerInv(fakeInv);

                cmd.CommandText = @"INSERT INTO characters (username, char_id, char_name, gender, skin_colour, race, 
                    hair_style, hair_colour, inventory)  VALUES ($username, $charIndex, $charName, $gender, $skinColour, $race, 
                    $hairStyle, $hairColour, $inventory);";

                cmd.Parameters.AddWithValue("$username", username);
                cmd.Parameters.AddWithValue("$charIndex", charIndex);
                cmd.Parameters.AddWithValue("$charName", charName);
                cmd.Parameters.AddWithValue("$gender", gender);
                cmd.Parameters.AddWithValue("$skinColour", skinColour);
                cmd.Parameters.AddWithValue("$race", race);
                cmd.Parameters.AddWithValue("$hairStyle", hairStyle);
                cmd.Parameters.AddWithValue("$hairColour", hairColour);
                cmd.Parameters.AddWithValue("$inventory", invString);

                int numRowsInserted = cmd.ExecuteNonQuery();
                bool created = (numRowsInserted > 0);
                CharacterDef def = null;

                if(created)
                {
                    def = new CharacterDef()
                    {
                        name = charName,
                        gender = gender,
                        skinColour = skinColour,
                        race = race,
                        hairStyle = hairStyle,
                        hairColour = hairColour,
                        doll = new Paperdoll
                        {
                            armor = 0,
                            hat = 0,
                            boots = 0,
                            back = 0,
                            weapon = 0
                        }

                    };
                }

                charDef = def;
                return created;
            }

            throw new Exception("Database closed");
        }

        public static CharacterDef[] GetCharDefs(string username)
        {
            if (conn.State == ConnectionState.Open)
            {
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"SELECT char_id, char_name, gender, skin_colour, race, hair_style, hair_colour, paperdoll FROM characters WHERE username = $username";
                cmd.Parameters.AddWithValue("$username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    CharacterDef[] defs = new CharacterDef[3];

                    int count = 0;
                    
                    while(reader.Read())
                    {
                        var charId = reader.GetByte(0);
                        var charName = reader.GetString(1);
                        var gender = reader.GetByte(2);
                        var skinColour = reader.GetByte(3);
                        var race = reader.GetByte(4);
                        var hairStyle = reader.GetByte(5);
                        var hairColour = reader.GetByte(6);
                        string paperdollString = null;

                        if (!reader.IsDBNull(7))
                            paperdollString = reader.GetString(7);

                        //Console.WriteLine($"Char {charName} id {charId}");
                        //TODO: Remove casting
                        defs[charId] = new CharacterDef
                        {
                            name = charName,
                            gender = gender,
                            skinColour = skinColour,
                            race =  race,
                            hairStyle = hairStyle,
                            hairColour = hairColour
                        };

                        //Load in paperdoll
                        if (paperdollString != null)
                            UtilFunctions.DeserializePaperdoll(defs[charId], paperdollString);

                        count++;
                    }
                    
                    if(count == 0)
                    {
                        Console.WriteLine($"No characters exist for user '{username}'exists.");
                        //return null;
                    }
                    
                    return defs;
                }
            }

            throw new Exception("Database closed");
        }

        public static DBCharacter GetDBChar(string charName) 
        {
            if (conn.State == ConnectionState.Open)
            {
                var cmd = conn.CreateCommand();
                DBCharacter dbChar = null;

                cmd.CommandText = @"SELECT health, mana, map, x, y, direction, inventory, paperdoll FROM characters WHERE char_name = $charName";
                cmd.Parameters.AddWithValue("$charName", charName);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dbChar = new DBCharacter();

                        if(!reader.IsDBNull(0))
                            dbChar.health = reader.GetInt64(0);
                        if (!reader.IsDBNull(1))
                            dbChar.mana = reader.GetInt64(1);
                        if (!reader.IsDBNull(2))
                            dbChar.map = reader.GetInt32(2);
                        if (!reader.IsDBNull(3))
                            dbChar.x = reader.GetInt32(3);
                        if (!reader.IsDBNull(4))
                            dbChar.y = reader.GetInt32(4);
                        if (!reader.IsDBNull(5))
                            dbChar.direction = reader.GetInt32(5);
                        if (!reader.IsDBNull(6))
                            dbChar.inventory = reader.GetString(6);
                        if (!reader.IsDBNull(7))
                            dbChar.paperdoll = reader.GetString(7);


                        
                    }

                    return dbChar;
                }
            }

            throw new Exception("Database closed");
        }

        public static void SaveCharacter(Character character)
        {
            if(Connected)
            {
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"UPDATE characters SET skin_colour = $skinColour, hair_style = $hairStyle, hair_colour = $hairColour, health = $health, mana = $mana, map = $map, x = $x, y = $y,
                    direction = $direction, inventory = $inventory, paperdoll = $paperdoll WHERE char_name = $charName;";

                object invString = UtilFunctions.SerializePlayerInv(character.inv);
                invString ??= DBNull.Value;

                cmd.Parameters.AddWithValue("$skinColour", character.def.skinColour);
                cmd.Parameters.AddWithValue("$hairStyle", character.def.hairStyle);
                cmd.Parameters.AddWithValue("$hairColour", character.def.hairColour);
                cmd.Parameters.AddWithValue("$health", character.props.health);
                cmd.Parameters.AddWithValue("$mana", character.props.mana);
                cmd.Parameters.AddWithValue("$map", character.map.mapId);
                cmd.Parameters.AddWithValue("$x", character.position.x);
                cmd.Parameters.AddWithValue("$y", character.position.y);
                cmd.Parameters.AddWithValue("$direction", character.direction);
                cmd.Parameters.AddWithValue("$inventory", invString);
                cmd.Parameters.AddWithValue("$paperdoll", UtilFunctions.SerializePlayerPaperdoll(character.def.doll));
                cmd.Parameters.AddWithValue("$charName", character.def.name);

                int numRowsUpdated = cmd.ExecuteNonQuery();

                return;
            }

            throw new Exception("Database closed");
        }

        //TODO: Update
        public static void LoadCharacterDef(string charName)
        {
            if(conn.State == ConnectionState.Open)
            {
                var cmd = conn.CreateCommand();

                cmd.CommandText = @"SELECT gender, race FROM characters WHERE char_name = $char_name";
                cmd.Parameters.AddWithValue("$char_name", charName);

                using(var reader = cmd.ExecuteReader())
                {
                   if(reader.Read())
                    {
                        var gender = reader.GetInt16(0);
                        var race = reader.GetInt16(1);
                        Console.WriteLine($"Loaded char gender: {gender}, race: {race}");
                    }
                   else
                    {
                        Console.WriteLine($"No character '{charName}'exists.");
                    }
                }
            }

            throw new Exception("Database closed");
        }
    }
}

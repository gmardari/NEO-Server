using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    //A useful class to calculate damage
    public static class CombatManager
    {
        //Used to calculate the damage dealth by an attacker's melee/ranged attack
        public static void EntityAttackEntity(Entity attacker, Entity victim)
        {
            if(victim is Mob mobVictim)
            {
                mobVictim.TakeDamage(1, attacker);
            }
            else if(victim is Character charVictim)
            {
                charVictim.TakeDamage(1, attacker);
            }
        }
    }

    public enum DamageSource
    {
        PLAYER,
        MOB
    }
}

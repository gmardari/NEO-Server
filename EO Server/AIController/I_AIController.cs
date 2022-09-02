using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public interface I_AIController
    {
        public Mob Mob { get; }
        public int Behaviour { get; }

        public void Update();
        public void OnTakeDamage(ulong damage, Entity attacker);
    }

    public enum MobBehaviour
    {
        FIDGET,
        PATROL,
        FOLLOW,
        ATTACK,
        RETURN
    }
}

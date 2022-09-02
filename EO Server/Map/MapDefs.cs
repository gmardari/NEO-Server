using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public enum MapLayer : int
    {
        GROUND,
        OBJECTS,
        OVERLAY,
        WALLS_DOWN,
        WALLS_RIGHT,
        SPECIAL
    }

    public enum MapSpecialIndex : int
    {
        WALL = 0,
        NPC_SPAWN = 37,
        WARP = 38,
        CHEST = 39
    }
}

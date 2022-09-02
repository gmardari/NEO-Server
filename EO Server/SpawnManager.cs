using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    //TODO: Overlapping id's
    public static class SpawnManager
    {
        private static ulong availableEntityId;
        
        public static ulong GetAvailableEntityId()
        {
            return availableEntityId++;
        }
    }
}

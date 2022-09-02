using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public struct Paperdoll
    {
        public uint hat;
        public uint armor;
        public uint weapon;
        public uint necklace;
        public uint back;
        public uint gloves;
        public uint belt;
        public uint charm;
        public uint boots;
        public uint ring1;
        public uint ring2;
        public uint bracelet1;
        public uint bracelet2;
        public uint bracer1;
        public uint bracer2;


        public uint GetItemId(PaperdollSlot slot)
        {
            uint itemId = 0;
            switch (slot)
            {

                case PaperdollSlot.HAT:
                    itemId = hat;
                    break;

                case PaperdollSlot.ARMOR:
                    itemId = armor;
                    break;

                case PaperdollSlot.WEAPON:
                    itemId = weapon;
                    break;

                case PaperdollSlot.NECKLACE:
                    itemId = necklace;
                    break;

                case PaperdollSlot.BACK:
                    itemId = back;
                    break;

                case PaperdollSlot.GLOVES:
                    itemId = gloves;
                    break;

                case PaperdollSlot.BELT:
                    itemId = belt;
                    break;
                case PaperdollSlot.CHARM:
                    itemId = charm;
                    break;

                case PaperdollSlot.BOOTS:
                    itemId = boots;
                    break;
                case PaperdollSlot.RING_1:
                    itemId = ring1;
                    break;
                case PaperdollSlot.RING_2:
                    itemId = ring2;
                    break;
                case PaperdollSlot.BRACELET_1:
                    itemId = bracelet1;
                    break;
                case PaperdollSlot.BRACELET_2:
                    itemId = bracelet2;
                    break;
                case PaperdollSlot.BRACER_1:
                    itemId = bracer1;
                    break;
                case PaperdollSlot.BRACER_2:
                    itemId = bracer2;
                    break;
            }

            return itemId;
        }

        public void Set(PaperdollSlot slot, uint val)
        {
            switch (slot)
            {

                case PaperdollSlot.HAT:
                    hat = val;
                    break;
                case PaperdollSlot.ARMOR:
                    armor = val;
                    break;
                case PaperdollSlot.WEAPON:
                    weapon = val;
                    break;
                case PaperdollSlot.NECKLACE:
                    necklace = val;
                    break;
                case PaperdollSlot.BACK:
                    back = val;
                    break;
                case PaperdollSlot.GLOVES:
                    gloves = val;
                    break;
                case PaperdollSlot.BELT:
                    belt = val;
                    break;
                case PaperdollSlot.CHARM:
                    charm = val;
                    break;
                case PaperdollSlot.BOOTS:
                    boots = val;
                    break;
                case PaperdollSlot.RING_1:
                    ring1 = val;
                    break;
                case PaperdollSlot.RING_2:
                    ring2 = val;
                    break;
                case PaperdollSlot.BRACELET_1:
                    bracelet1 = val;
                    break;
                case PaperdollSlot.BRACELET_2:
                    bracelet2 = val;
                    break;
                case PaperdollSlot.BRACER_1:
                    bracer1 = val;
                    break;
                case PaperdollSlot.BRACER_2:
                    bracer2 = val;
                    break;
            }
        }
    }

    public enum PaperdollSlot
    {
        HAT,
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
    }
}

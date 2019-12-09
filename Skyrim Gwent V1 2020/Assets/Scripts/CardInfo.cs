using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardInfo
{
    public enum UnitType { Warrior,Mage,Spellsword,Shadow};
    public enum SubUnitType { HeavyArmor, LightArmor, FireMage, FrostMage, LightningMage, Conjurer};


    public enum Faction { Whiterun, Riften, Dark_Brotherhood};

    

    public string name;
    public int strength;
    public int originalStrength;       // incase of buffs and debuffs

    public bool isHero;          // doesnt affect heros

    public UnitType unitType = new UnitType();
    public Faction faction = new Faction();
    public SubUnitType subUnitType = new SubUnitType();



    // functions
    public void AddBuff(int buff)
    {
        if(!isHero)
            strength += buff;
    }

    public void AddDeBuff(int debuff)
    {
        if (!isHero)
        {
            strength -= debuff;
            if (strength < 1)
                strength = 1;
        }
               
    }   

}

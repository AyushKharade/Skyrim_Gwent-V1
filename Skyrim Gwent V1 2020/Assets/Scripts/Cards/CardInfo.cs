using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardInfo
{
    public enum UnitType { Warrior,Mage,Spellsword,Shadow};
    public enum SubUnitType { HeavyArmor, LightArmor, FireMage, FrostMage, LightningMage, Conjurer, Archer, Assassin, Thief};


    public enum Faction { Whiterun, Riften, Dark_Brotherhood};


    public enum TrainingLevel { Novice, Apprentice, Adept, Expert, Master};

    

    public string name;
    [Range(0,15)]
    public int strength;
    [Range(0, 15)]
    public int originalStrength;       // incase of buffs and debuffs

    public bool isHero;          // doesnt affect heros


    public TrainingLevel trainingLevel = new TrainingLevel();

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardInfo
{
    public enum UnitType { Warrior,Mage,Spellsword,Shadow,Special};
    public enum SubUnitType { HeavyArmor, LightArmor, FireMage, FrostMage, LightningMage, Conjurer, Archer, Assassin, Thief, Healer, Spy,
        FrostWeather,BaneAetheriusWeather,StormWeather,ClearWeather};

    public enum Race { Imperial, Nord, Redguard, Khajit, Argonian, Wood_Elf, Dark_Elf, Vampire, Breton, Draugr };

    public enum Faction { Whiterun, Riften, Dark_Brotherhood, Draugr};


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
    public Race race=new Race();

    [TextArea]
    public string Ability_Details;

    [TextArea]
    public string Quotes;

    // functions
    public void AddBuff(int buff)
    {
        if (!isHero)
        {
            strength += buff;
        }
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

    public void ResetBuffs()
    {
        strength = originalStrength;
    }


    //getters
    public string GetUnitType()
    {
        return unitType + "";
    }

    public string GetSubUnitType()
    {
        return subUnitType + "";
    }
}

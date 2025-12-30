using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Character_Sheet", menuName = "Character", order = 1)]
public class CharacterSheet : ScriptableObject
{
    public enum Race
    {
        Human,
        Elf,
        HalfElf,
        Dwarf,
        Halfling,
        FiendFolk,
        SatyrFolk,
        Dhampir,
        MothFolk
    }
    public Race race;
    
    [Serializable] public class CoreAttribute
    {
        [Header("Core Attributes")]
        public int charisma = 2;
        public int finesse = 2;
        public int fortitude = 2;
        public int intellect = 2;
        public int resolve = 2;
        public int strength = 2;

        [Header("Derived Attributes")]
        public int health = 2;              //Fortitde + 1/2 Strength + 6
        public int initiative = 2;          //Finesse + Awareness
        public int willpower = 2;           //Intellect + Resolve
        public int mana = 2;                //Charisma + Intelect + Resolve
        public int manaRegeneration = 2;    //Resolve
        public int speed = 2;               //Strength + Finesse + Athletics
    }

    [Serializable] public class Skills
    {
        [Serializable] public class CharismaSkill
        {
            public int animalKen = 2;
            public int diplomacy = 2;
            public int deception = 2;
            public int intimidation = 2;
            public int performance = 2;
        }
        [Serializable] public class FinesseSkill
        {
            public int acrobatics = 2;
            public int bluntWeapons = 2;
            public int heavyBlades = 2;
            public int lightBlades = 2;
            public int marksmanship = 2;
            public int pugilism = 2;
            public int ride = 2;
            public int stealth = 2;
            public int thievery = 2;
        }
        [Serializable] public class IntellectSkill
        {
            public int academics = 2;
            public int arcana = 2;
            public int awareness = 2;
            public int crafting = 2;
            public int deduction = 2;
            public int herbalism = 2;
            public int survival = 2;
        }
        [Serializable] public class StrengthSkill
        {
            public int athletics = 2;
        }
        [SerializeField] public FinesseSkill finesseSkill;
        [SerializeField] public IntellectSkill intellectSkill;
        [SerializeField] public StrengthSkill strengthSkill;
    }
    [SerializeField] public CoreAttribute coreAttributes; 
    [SerializeField] public Skills skills;

    [ContextMenu("Calculate Derivatives")]
    private void CalculateDerivatives()
    {
        coreAttributes.health = coreAttributes.fortitude + Mathf.FloorToInt( 2 / coreAttributes.strength ) + 6;
        coreAttributes.initiative = coreAttributes.finesse + skills.intellectSkill.awareness;
        coreAttributes.willpower = coreAttributes.intellect + coreAttributes.resolve;
        coreAttributes.mana = coreAttributes.charisma + coreAttributes.intellect + coreAttributes.resolve;
        coreAttributes.manaRegeneration = coreAttributes.resolve;
        coreAttributes.speed = coreAttributes.strength + coreAttributes.finesse + skills.strengthSkill.athletics;
    }
}

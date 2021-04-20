namespace MHRTalismanManager.Shared
{
    public class Skill
    {
        public string Name { get; set; }

        public SlotType? JewelType { get; set; }

        public Rarity Rarity { get; set; }

        public int MaxPoints { get; set; }

        public string Description { get; set; }
    }
}

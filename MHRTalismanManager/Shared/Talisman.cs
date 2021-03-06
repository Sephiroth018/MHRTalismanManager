namespace MHRTalismanManager.Shared
{
    public class Talisman
    {
        public TalismanSkill Skill1 { get; set; } = new();

        public TalismanSkill Skill2 { get; set; } = new();

        public SlotType Slot1 { get; set; } = SlotType.None;

        public SlotType Slot2 { get; set; } = SlotType.None;

        public SlotType Slot3 { get; set; } = SlotType.None;

        public TalismanStatus TalismanStatus { get; set; } = TalismanStatus.Unevaluated;
    }
}

using System.ComponentModel.DataAnnotations;

namespace MHRTalismanManager.Shared
{
    public enum SlotType
    {
        [Display(Name = "-")] None = 0,
        [Display(Name = "1")] Level1 = 1,
        [Display(Name = "2")] Level2 = 2,
        [Display(Name = "3")] Level3 = 3,
    }
}

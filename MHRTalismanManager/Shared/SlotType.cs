using System.ComponentModel.DataAnnotations;

namespace MHRTalismanManager.Shared
{
    public enum SlotType
    {
        [Display(Name = "-")] None,
        [Display(Name = "1")] Level1,
        [Display(Name = "2")] Level2,
        [Display(Name = "3")] Level3,
    }
}

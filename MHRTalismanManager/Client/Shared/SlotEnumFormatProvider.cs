using System;
using MHRTalismanManager.Shared;

namespace MHRTalismanManager.Client.Shared
{
    public class SlotEnumFormatProvider : IFormatProvider, ICustomFormatter
    {
        public string Format(string? format, object? arg, IFormatProvider? formatProvider)
        {
            if (!Equals(formatProvider, this))
                return null;
            if (arg is SlotType slotType)
                return slotType.GetDisplayName();
            throw new NotImplementedException();
        }

        public object? GetFormat(Type? formatType)
        {
            return formatType == typeof(ICustomFormatter)
                       ? this
                       : null;
        }
    }
}

using System;

namespace Richter.Utilities
{
    public static class EnumEx {
        public static TEnum[] GetValues<TEnum>() where TEnum : struct {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }
        public static TEnum Parse<TEnum>(string value, bool ignoreCase = false) where TEnum : struct {
            return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
        }
    }
}
namespace RetailBank.Extensions;

public static class UInt128Ext
{
    public static string ToHex(this UInt128 value)
    {
        return value.ToString("X").PadLeft(32, '0');
    }
}

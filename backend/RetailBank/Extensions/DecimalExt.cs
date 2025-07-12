namespace RetailBank.Extensions;

public static class DecimalExt
{
    public static decimal Pow(this decimal value, int power)
    {
        var out_value = 1.0m;

        if (power >= 0)
            for (int i = 0; i < power; i++)
                out_value *= value;
        else
            for (int i = 0; i < -power; i++)
                out_value /= value;

        return out_value;
    }
}

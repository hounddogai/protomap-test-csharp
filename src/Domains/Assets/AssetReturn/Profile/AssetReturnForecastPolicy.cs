namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of asset return forecast policy.</summary>
public static class AssetReturnForecastPolicy
{
    public static int AssetReturnForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    public static string AssetReturnForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}

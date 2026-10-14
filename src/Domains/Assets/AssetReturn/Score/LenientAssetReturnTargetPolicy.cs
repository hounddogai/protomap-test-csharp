namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient asset return target policy.</summary>
public static class LenientAssetReturnTargetPolicy
{
    public static string LenientAssetReturnTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double LenientAssetReturnTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LenientAssetReturnTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}

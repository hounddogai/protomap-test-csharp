namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped laptop manifest policy.</summary>
public static class CappedLaptopManifestPolicy
{
    public static double CappedLaptopManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int CappedLaptopManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}

namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default laptop segment policy.</summary>
public static class DefaultLaptopSegmentPolicy
{
    public static double DefaultLaptopSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a default laptop segment code in its canonical DLS- form.</summary>
    public static string NormalizeDefaultLaptopSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DLS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DLS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop checkpoint policy.</summary>
public static class LaptopCheckpointPolicy
{
    public static double LaptopCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a laptop checkpoint code in its canonical LC- form.</summary>
    public static string NormalizeLaptopCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLaptopCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

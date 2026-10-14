namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted desk preference policy.</summary>
public static class WeightedDeskPreferencePolicy
{
    public static double WeightedDeskPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a weighted desk preference code in its canonical WDP- form.</summary>
    public static string NormalizeWeightedDeskPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WDP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WDP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

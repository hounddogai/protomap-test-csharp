namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of assignment preference policy.</summary>
public static class AssignmentPreferencePolicy
{
    public static double AssignmentPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AssignmentPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a assignment preference code in its canonical AP- form.</summary>
    public static string NormalizeAssignmentPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

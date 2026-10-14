namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped position assignment policy.</summary>
public static class CappedPositionAssignmentPolicy
{
    /// <summary>Names the band a capped position assignment score falls in.</summary>
    public static string ClassifyCappedPositionAssignment(double score)
    {
        if (score < 12.6)
        {
            return "critical";
        }
        if (score < 44.4)
        {
            return "medium";
        }
        if (score < 47.2)
        {
            return "high";
        }
        return "minimal";
    }

    public static string CappedPositionAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double CappedPositionAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.23;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a capped position assignment code in its canonical CPA- form.</summary>
    public static string NormalizeCappedPositionAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

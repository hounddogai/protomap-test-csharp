namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception milestone policy.</summary>
public static class ExceptionMilestonePolicy
{
    public static double ExceptionMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ExceptionMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a exception milestone code in its canonical EM- form.</summary>
    public static string NormalizeExceptionMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

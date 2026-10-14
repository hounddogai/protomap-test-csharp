namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled statement of work window policy.</summary>
public static class ScheduledStatementOfWorkWindowPolicy
{
    /// <summary>Puts a scheduled statement of work window code in its canonical SSOWW- form.</summary>
    public static string NormalizeScheduledStatementOfWorkWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSOWW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSOWW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledStatementOfWorkWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledStatementOfWorkWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a scheduled statement of work window score falls in.</summary>
    public static string ClassifyScheduledStatementOfWorkWindow(double score)
    {
        if (score < 11.8)
        {
            return "elevated";
        }
        if (score < 12.0)
        {
            return "minimal";
        }
        if (score < 61.3)
        {
            return "critical";
        }
        return "high";
    }
}

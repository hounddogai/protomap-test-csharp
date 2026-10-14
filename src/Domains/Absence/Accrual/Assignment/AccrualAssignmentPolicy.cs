namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual assignment policy.</summary>
public static class AccrualAssignmentPolicy
{
    public static int AccrualAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a accrual assignment code in its canonical AA- form.</summary>
    public static string NormalizeAccrualAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AccrualAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static double AccrualAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

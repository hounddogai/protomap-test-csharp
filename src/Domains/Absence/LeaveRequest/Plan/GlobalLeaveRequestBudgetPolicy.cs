namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global leave request budget policy.</summary>
public static class GlobalLeaveRequestBudgetPolicy
{
    /// <summary>Puts a global leave request budget code in its canonical GLRB- form.</summary>
    public static string NormalizeGlobalLeaveRequestBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GLRB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GLRB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double GlobalLeaveRequestBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string GlobalLeaveRequestBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int GlobalLeaveRequestBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}

namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request quota policy.</summary>
public static class LeaveRequestQuotaPolicy
{
    public static string LeaveRequestQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 32857.0, in order.</summary>
    public static int LeaveRequestQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32857.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a leave request quota code in its canonical LRQ- form.</summary>
    public static string NormalizeLeaveRequestQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LeaveRequestQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

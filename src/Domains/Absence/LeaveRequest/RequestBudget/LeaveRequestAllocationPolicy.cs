namespace Acme.Workforce.Absence.LeaveRequest.RequestBudget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request allocation policy.</summary>
public static class LeaveRequestAllocationPolicy
{
    /// <summary>Puts a leave request allocation code in its canonical LRA- form.</summary>
    public static string NormalizeLeaveRequestAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LeaveRequestAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double LeaveRequestAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 16134.0, in order.</summary>
    public static int LeaveRequestAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16134.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

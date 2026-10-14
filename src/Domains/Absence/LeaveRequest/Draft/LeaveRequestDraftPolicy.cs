namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of leave request draft policy.</summary>
public static class LeaveRequestDraftPolicy
{
    public static string LeaveRequestDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 8752.0, in order.</summary>
    public static int LeaveRequestDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8752.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a leave request draft code in its canonical LRD- form.</summary>
    public static string NormalizeLeaveRequestDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

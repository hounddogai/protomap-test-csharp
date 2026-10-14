namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary leave request item policy.</summary>
public static class PrimaryLeaveRequestItemPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 34610.0, in order.</summary>
    public static int PrimaryLeaveRequestItemBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 34610.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string PrimaryLeaveRequestItemBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

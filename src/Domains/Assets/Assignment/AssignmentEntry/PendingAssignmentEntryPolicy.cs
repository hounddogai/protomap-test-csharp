namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending assignment entry policy.</summary>
public static class PendingAssignmentEntryPolicy
{
    public static string PendingAssignmentEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int PendingAssignmentEntrySpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 74507.0, in order.</summary>
    public static int PendingAssignmentEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74507.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

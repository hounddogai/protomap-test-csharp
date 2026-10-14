namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback exception assignment policy.</summary>
public static class FallbackExceptionAssignmentPolicy
{
    public static int FallbackExceptionAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string FallbackExceptionAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 72979.0, in order.</summary>
    public static int FallbackExceptionAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72979.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

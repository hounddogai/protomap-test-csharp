namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict span slot policy.</summary>
public static class StrictSpanSlotPolicy
{
    public static int StrictSpanSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 25035.0, in order.</summary>
    public static int StrictSpanSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25035.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string StrictSpanSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

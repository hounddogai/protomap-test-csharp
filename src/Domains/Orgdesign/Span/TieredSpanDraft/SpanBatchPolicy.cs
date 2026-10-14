namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Functions of span batch policy.</summary>
public static class SpanBatchPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 21247.0, in order.</summary>
    public static int SpanBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21247.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int SpanBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }
}

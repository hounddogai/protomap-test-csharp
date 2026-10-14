namespace Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Functions of weighted recognition threshold policy.</summary>
public static class WeightedRecognitionThresholdPolicy
{
    public static int WeightedRecognitionThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 15346.0, in order.</summary>
    public static int WeightedRecognitionThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15346.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

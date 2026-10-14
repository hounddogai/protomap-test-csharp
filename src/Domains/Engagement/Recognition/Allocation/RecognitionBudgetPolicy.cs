namespace Acme.Workforce.Engagement.Recognition.Allocation;

/// <summary>Functions of recognition budget policy.</summary>
public static class RecognitionBudgetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 78744.0, in order.</summary>
    public static int RecognitionBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78744.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RecognitionBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }
}

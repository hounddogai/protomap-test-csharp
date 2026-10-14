namespace Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Functions of recognition profile policy.</summary>
public static class RecognitionProfilePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 1162.0, in order.</summary>
    public static int RecognitionProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1162.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RecognitionProfileSpan(int start, int end)
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
        return span / 60;
    }
}

namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Functions of reorg window policy.</summary>
public static class ReorgWindowPolicy
{
    public static int ReorgWindowSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 85825.0, in order.</summary>
    public static int ReorgWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85825.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

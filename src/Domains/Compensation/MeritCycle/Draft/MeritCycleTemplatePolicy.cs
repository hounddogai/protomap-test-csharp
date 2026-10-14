namespace Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Functions of merit cycle template policy.</summary>
public static class MeritCycleTemplatePolicy
{
    public static int MeritCycleTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 57928.0, in order.</summary>
    public static int MeritCycleTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 57928.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

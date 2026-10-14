namespace Acme.Workforce.Talent.Nomination.Template;

/// <summary>Functions of nomination template policy.</summary>
public static class NominationTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 95746.0, in order.</summary>
    public static int NominationTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95746.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int NominationTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}

namespace Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Functions of certification slot policy.</summary>
public static class CertificationSlotPolicy
{
    public static int CertificationSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 55761.0, in order.</summary>
    public static int CertificationSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55761.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

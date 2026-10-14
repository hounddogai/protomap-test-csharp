namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Functions of legacy pay run agreement policy.</summary>
public static class LegacyPayRunAgreementPolicy
{
    public static int LegacyPayRunAgreementSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 48583.0, in order.</summary>
    public static int LegacyPayRunAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48583.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

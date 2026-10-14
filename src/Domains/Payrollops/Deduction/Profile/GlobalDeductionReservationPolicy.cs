namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global deduction reservation policy.</summary>
public static class GlobalDeductionReservationPolicy
{
    /// <summary>Names the band a global deduction reservation score falls in.</summary>
    public static string ClassifyGlobalDeductionReservation(double score)
    {
        if (score < 24.3)
        {
            return "minimal";
        }
        if (score < 72.2)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 49372.0, in order.</summary>
    public static int GlobalDeductionReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49372.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string GlobalDeductionReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int GlobalDeductionReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}

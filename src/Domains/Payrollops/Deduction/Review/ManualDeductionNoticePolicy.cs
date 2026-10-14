namespace Acme.Workforce.Payrollops.Deduction.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual deduction notice policy.</summary>
public static class ManualDeductionNoticePolicy
{
    public static string ManualDeductionNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 38603.0, in order.</summary>
    public static int ManualDeductionNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38603.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

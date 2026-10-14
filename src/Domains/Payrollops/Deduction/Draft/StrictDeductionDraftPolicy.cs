namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict deduction draft policy.</summary>
public static class StrictDeductionDraftPolicy
{
    /// <summary>Names the band a strict deduction draft score falls in.</summary>
    public static string ClassifyStrictDeductionDraft(double score)
    {
        if (score < 49.8)
        {
            return "minimal";
        }
        if (score < 51.6)
        {
            return "ok";
        }
        return "medium";
    }

    public static string StrictDeductionDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 20586.0, in order.</summary>
    public static int StrictDeductionDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20586.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

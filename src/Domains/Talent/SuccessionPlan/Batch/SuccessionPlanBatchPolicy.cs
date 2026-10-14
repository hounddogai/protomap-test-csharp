namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan batch policy.</summary>
public static class SuccessionPlanBatchPolicy
{
    /// <summary>Puts a succession plan batch code in its canonical SPB- form.</summary>
    public static string NormalizeSuccessionPlanBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 88148.0, in order.</summary>
    public static int SuccessionPlanBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88148.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SuccessionPlanBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}

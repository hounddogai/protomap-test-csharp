namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay run segment policy.</summary>
public static class PayRunSegmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 12855.0, in order.</summary>
    public static int PayRunSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12855.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergePayRunSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a pay run segment score falls in.</summary>
    public static string ClassifyPayRunSegment(double score)
    {
        if (score < 7.0)
        {
            return "elevated";
        }
        if (score < 46.8)
        {
            return "watch";
        }
        if (score < 82.6)
        {
            return "ok";
        }
        return "low";
    }

    public static string PayRunSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

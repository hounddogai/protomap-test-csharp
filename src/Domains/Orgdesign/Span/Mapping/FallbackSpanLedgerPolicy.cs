namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback span ledger policy.</summary>
public static class FallbackSpanLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 39192.0, in order.</summary>
    public static int FallbackSpanLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39192.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string FallbackSpanLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeFallbackSpanLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a fallback span ledger score falls in.</summary>
    public static string ClassifyFallbackSpanLedger(double score)
    {
        if (score < 6.9)
        {
            return "high";
        }
        if (score < 7.9)
        {
            return "minimal";
        }
        if (score < 68.3)
        {
            return "medium";
        }
        if (score < 82.5)
        {
            return "low";
        }
        return "watch";
    }
}

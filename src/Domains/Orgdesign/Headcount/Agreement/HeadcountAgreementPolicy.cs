namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of headcount agreement policy.</summary>
public static class HeadcountAgreementPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 98719.0, in order.</summary>
    public static int HeadcountAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98719.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string HeadcountAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a headcount agreement score falls in.</summary>
    public static string ClassifyHeadcountAgreement(double score)
    {
        if (score < 3.1)
        {
            return "minimal";
        }
        if (score < 14.4)
        {
            return "elevated";
        }
        if (score < 92.9)
        {
            return "critical";
        }
        return "high";
    }

    public static List<string> MergeHeadcountAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

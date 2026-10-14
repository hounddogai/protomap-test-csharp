namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional offboarding review policy.</summary>
public static class RegionalOffboardingReviewPolicy
{
    /// <summary>Names the band a regional offboarding review score falls in.</summary>
    public static string ClassifyRegionalOffboardingReview(double score)
    {
        if (score < 57.4)
        {
            return "elevated";
        }
        if (score < 60.7)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 10498.0, in order.</summary>
    public static int RegionalOffboardingReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10498.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeRegionalOffboardingReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

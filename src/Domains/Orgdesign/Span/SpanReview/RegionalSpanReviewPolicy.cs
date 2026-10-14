namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional span review policy.</summary>
public static class RegionalSpanReviewPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 78638.0, in order.</summary>
    public static int RegionalSpanReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78638.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RegionalSpanReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a regional span review score falls in.</summary>
    public static string ClassifyRegionalSpanReview(double score)
    {
        if (score < 19.8)
        {
            return "elevated";
        }
        if (score < 58.2)
        {
            return "minimal";
        }
        return "medium";
    }
}

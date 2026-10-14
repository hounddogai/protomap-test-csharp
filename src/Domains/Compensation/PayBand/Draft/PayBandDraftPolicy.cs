namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band draft policy.</summary>
public static class PayBandDraftPolicy
{
    /// <summary>Names the band a pay band draft score falls in.</summary>
    public static string ClassifyPayBandDraft(double score)
    {
        if (score < 11.3)
        {
            return "watch";
        }
        if (score < 43.1)
        {
            return "high";
        }
        if (score < 51.4)
        {
            return "medium";
        }
        return "elevated";
    }

    public static string PayBandDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int PayBandDraftSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 71226.0, in order.</summary>
    public static int PayBandDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71226.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

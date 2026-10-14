namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted refresh score policy.</summary>
public static class WeightedRefreshScorePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99016.0, in order.</summary>
    public static int WeightedRefreshScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99016.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string WeightedRefreshScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a weighted refresh score code in its canonical WRS- form.</summary>
    public static string NormalizeWeightedRefreshScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int WeightedRefreshScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}

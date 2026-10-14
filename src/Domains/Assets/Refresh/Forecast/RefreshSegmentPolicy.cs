namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of refresh segment policy.</summary>
public static class RefreshSegmentPolicy
{
    /// <summary>Puts a refresh segment code in its canonical RS- form.</summary>
    public static string NormalizeRefreshSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RefreshSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 83311.0, in order.</summary>
    public static int RefreshSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 83311.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

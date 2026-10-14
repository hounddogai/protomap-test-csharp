namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending occupancy notice policy.</summary>
public static class PendingOccupancyNoticePolicy
{
    /// <summary>Puts a pending occupancy notice code in its canonical PON- form.</summary>
    public static string NormalizePendingOccupancyNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PON-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PON-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PendingOccupancyNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 82235.0, in order.</summary>
    public static int PendingOccupancyNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82235.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

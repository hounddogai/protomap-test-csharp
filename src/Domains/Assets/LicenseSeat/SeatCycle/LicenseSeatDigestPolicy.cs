namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat digest policy.</summary>
public static class LicenseSeatDigestPolicy
{
    public static string LicenseSeatDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a license seat digest code in its canonical LSD- form.</summary>
    public static string NormalizeLicenseSeatDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LicenseSeatDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 21436.0, in order.</summary>
    public static int LicenseSeatDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21436.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

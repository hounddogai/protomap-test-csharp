namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending license seat checkpoint policy.</summary>
public static class PendingLicenseSeatCheckpointPolicy
{
    /// <summary>Puts a pending license seat checkpoint code in its canonical PLSC- form.</summary>
    public static string NormalizePendingLicenseSeatCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PLSC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PLSC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PendingLicenseSeatCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a pending license seat checkpoint score falls in.</summary>
    public static string ClassifyPendingLicenseSeatCheckpoint(double score)
    {
        if (score < 61.1)
        {
            return "critical";
        }
        if (score < 90.5)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 31849.0, in order.</summary>
    public static int PendingLicenseSeatCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31849.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

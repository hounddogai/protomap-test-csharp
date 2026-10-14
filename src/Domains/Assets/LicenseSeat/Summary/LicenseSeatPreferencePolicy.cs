namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat preference policy.</summary>
public static class LicenseSeatPreferencePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 53778.0, in order.</summary>
    public static int LicenseSeatPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53778.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a license seat preference score falls in.</summary>
    public static string ClassifyLicenseSeatPreference(double score)
    {
        if (score < 48.2)
        {
            return "critical";
        }
        if (score < 49.4)
        {
            return "minimal";
        }
        if (score < 63.9)
        {
            return "elevated";
        }
        if (score < 79.1)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Puts a license seat preference code in its canonical LSP- form.</summary>
    public static string NormalizeLicenseSeatPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LicenseSeatPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

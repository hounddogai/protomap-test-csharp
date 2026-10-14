namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Functions of scheduled license seat quota policy.</summary>
public static class ScheduledLicenseSeatQuotaPolicy
{
    /// <summary>Puts a scheduled license seat quota code in its canonical SLSQ- form.</summary>
    public static string NormalizeScheduledLicenseSeatQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SLSQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SLSQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 9871.0, in order.</summary>
    public static int ScheduledLicenseSeatQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9871.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

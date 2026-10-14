namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient license seat schedule policy.</summary>
public static class LenientLicenseSeatSchedulePolicy
{
    public static string LenientLicenseSeatScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a lenient license seat schedule code in its canonical LLSS- form.</summary>
    public static string NormalizeLenientLicenseSeatScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLSS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLSS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 97951.0, in order.</summary>
    public static int LenientLicenseSeatScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97951.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient license seat schedule score falls in.</summary>
    public static string ClassifyLenientLicenseSeatSchedule(double score)
    {
        if (score < 35.9)
        {
            return "low";
        }
        if (score < 49.8)
        {
            return "ok";
        }
        if (score < 55.1)
        {
            return "elevated";
        }
        if (score < 66.6)
        {
            return "minimal";
        }
        return "high";
    }
}

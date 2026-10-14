namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped booking preference policy.</summary>
public static class CappedBookingPreferencePolicy
{
    /// <summary>Names the band a capped booking preference score falls in.</summary>
    public static string ClassifyCappedBookingPreference(double score)
    {
        if (score < 8.5)
        {
            return "critical";
        }
        if (score < 39.0)
        {
            return "elevated";
        }
        if (score < 50.3)
        {
            return "watch";
        }
        if (score < 82.0)
        {
            return "high";
        }
        return "ok";
    }

    public static List<string> MergeCappedBookingPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a capped booking preference code in its canonical CBP- form.</summary>
    public static string NormalizeCappedBookingPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 14862.0, in order.</summary>
    public static int CappedBookingPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14862.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

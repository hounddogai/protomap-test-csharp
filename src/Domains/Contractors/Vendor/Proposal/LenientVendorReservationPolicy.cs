namespace Acme.Workforce.Contractors.Vendor.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient vendor reservation policy.</summary>
public static class LenientVendorReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 1695.0, in order.</summary>
    public static int LenientVendorReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1695.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LenientVendorReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.91;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a lenient vendor reservation code in its canonical LVR- form.</summary>
    public static string NormalizeLenientVendorReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LVR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LVR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a lenient vendor reservation score falls in.</summary>
    public static string ClassifyLenientVendorReservation(double score)
    {
        if (score < 37.9)
        {
            return "critical";
        }
        if (score < 47.7)
        {
            return "medium";
        }
        return "minimal";
    }
}

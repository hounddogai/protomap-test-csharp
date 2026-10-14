namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of license seat quota policy.</summary>
public static class LicenseSeatQuotaPolicy
{
    /// <summary>Names the band a license seat quota score falls in.</summary>
    public static string ClassifyLicenseSeatQuota(double score)
    {
        if (score < 39.1)
        {
            return "elevated";
        }
        if (score < 72.3)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 86267.0, in order.</summary>
    public static int LicenseSeatQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86267.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LicenseSeatQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeLicenseSeatQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

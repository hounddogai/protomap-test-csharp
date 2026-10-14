namespace Acme.Workforce.Contractors.Vendor.ScheduledVendorSchedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional vendor schedule policy.</summary>
public static class RegionalVendorSchedulePolicy
{
    public static string RegionalVendorScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeRegionalVendorScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a regional vendor schedule score falls in.</summary>
    public static string ClassifyRegionalVendorSchedule(double score)
    {
        if (score < 61.5)
        {
            return "minimal";
        }
        if (score < 66.3)
        {
            return "elevated";
        }
        if (score < 70.6)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 12936.0, in order.</summary>
    public static int RegionalVendorScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12936.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

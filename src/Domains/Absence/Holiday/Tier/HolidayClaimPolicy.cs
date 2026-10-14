namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of holiday claim policy.</summary>
public static class HolidayClaimPolicy
{
    /// <summary>Names the band a holiday claim score falls in.</summary>
    public static string ClassifyHolidayClaim(double score)
    {
        if (score < 48.1)
        {
            return "high";
        }
        if (score < 90.0)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHolidayClaim()
    {
        var policy = Retry.DefaultPolicy("holiday-claim");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeHolidayClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 9834.0, in order.</summary>
    public static int HolidayClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9834.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

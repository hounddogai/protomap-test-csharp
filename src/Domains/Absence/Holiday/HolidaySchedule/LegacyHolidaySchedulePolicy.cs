namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy holiday schedule policy.</summary>
public static class LegacyHolidaySchedulePolicy
{
    public static List<string> MergeLegacyHolidayScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyHolidaySchedule()
    {
        var policy = Retry.DefaultPolicy("legacy-holiday-schedule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 72607.0, in order.</summary>
    public static int LegacyHolidayScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72607.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy holiday schedule score falls in.</summary>
    public static string ClassifyLegacyHolidaySchedule(double score)
    {
        if (score < 46.8)
        {
            return "low";
        }
        if (score < 88.6)
        {
            return "high";
        }
        return "ok";
    }
}

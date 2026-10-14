namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool schedule policy.</summary>
public static class BonusPoolSchedulePolicy
{
    /// <summary>Names the band a bonus pool schedule score falls in.</summary>
    public static string ClassifyBonusPoolSchedule(double score)
    {
        if (score < 7.5)
        {
            return "low";
        }
        if (score < 23.4)
        {
            return "high";
        }
        if (score < 63.0)
        {
            return "watch";
        }
        if (score < 69.7)
        {
            return "minimal";
        }
        return "critical";
    }

    public static List<string> MergeBonusPoolScheduleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBonusPoolSchedule()
    {
        var policy = Retry.DefaultPolicy("bonus-pool-schedule");
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
}

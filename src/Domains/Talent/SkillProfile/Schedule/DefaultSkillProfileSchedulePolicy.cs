namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default skill profile schedule policy.</summary>
public static class DefaultSkillProfileSchedulePolicy
{
    /// <summary>Names the band a default skill profile schedule score falls in.</summary>
    public static string ClassifyDefaultSkillProfileSchedule(double score)
    {
        if (score < 17.4)
        {
            return "low";
        }
        if (score < 37.7)
        {
            return "critical";
        }
        if (score < 91.2)
        {
            return "minimal";
        }
        return "medium";
    }

    public static string DefaultSkillProfileScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultSkillProfileSchedule()
    {
        var policy = Retry.DefaultPolicy("default-skill-profile-schedule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 23134.0, in order.</summary>
    public static int DefaultSkillProfileScheduleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 23134.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

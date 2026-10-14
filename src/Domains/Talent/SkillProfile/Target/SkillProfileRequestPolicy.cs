namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of skill profile request policy.</summary>
public static class SkillProfileRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSkillProfileRequest()
    {
        var policy = Retry.DefaultPolicy("skill-profile-request");
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

    /// <summary>Names the band a skill profile request score falls in.</summary>
    public static string ClassifySkillProfileRequest(double score)
    {
        if (score < 23.0)
        {
            return "medium";
        }
        if (score < 31.2)
        {
            return "watch";
        }
        if (score < 45.1)
        {
            return "minimal";
        }
        if (score < 50.5)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 29936.0, in order.</summary>
    public static int SkillProfileRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29936.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

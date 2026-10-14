namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched skill profile notice policy.</summary>
public static class BatchedSkillProfileNoticePolicy
{
    /// <summary>Names the band a batched skill profile notice score falls in.</summary>
    public static string ClassifyBatchedSkillProfileNotice(double score)
    {
        if (score < 49.6)
        {
            return "medium";
        }
        if (score < 61.3)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 21278.0, in order.</summary>
    public static int BatchedSkillProfileNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21278.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedSkillProfileNotice()
    {
        var policy = Retry.DefaultPolicy("batched-skill-profile-notice");
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

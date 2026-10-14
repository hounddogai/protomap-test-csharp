namespace Acme.Workforce.Talent.CareerPath.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling career path profile policy.</summary>
public static class RollingCareerPathProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingCareerPathProfile()
    {
        var policy = Retry.DefaultPolicy("rolling-career-path-profile");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a rolling career path profile score falls in.</summary>
    public static string ClassifyRollingCareerPathProfile(double score)
    {
        if (score < 13.6)
        {
            return "elevated";
        }
        if (score < 70.0)
        {
            return "high";
        }
        return "minimal";
    }

    public static List<string> MergeRollingCareerPathProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 3614.0, in order.</summary>
    public static int RollingCareerPathProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 3614.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

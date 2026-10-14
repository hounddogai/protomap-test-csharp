namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending skill profile digest policy.</summary>
public static class PendingSkillProfileDigestPolicy
{
    /// <summary>Puts a pending skill profile digest code in its canonical PSPD- form.</summary>
    public static string NormalizePendingSkillProfileDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PendingSkillProfileDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingSkillProfileDigest()
    {
        var policy = Retry.DefaultPolicy("pending-skill-profile-digest");
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

    /// <summary>Names the band a pending skill profile digest score falls in.</summary>
    public static string ClassifyPendingSkillProfileDigest(double score)
    {
        if (score < 46.6)
        {
            return "minimal";
        }
        if (score < 60.7)
        {
            return "low";
        }
        if (score < 95.1)
        {
            return "high";
        }
        return "watch";
    }
}

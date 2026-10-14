namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict skill profile entry policy.</summary>
public static class StrictSkillProfileEntryPolicy
{
    /// <summary>Puts a strict skill profile entry code in its canonical SSPE- form.</summary>
    public static string NormalizeStrictSkillProfileEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int StrictSkillProfileEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictSkillProfileEntry()
    {
        var policy = Retry.DefaultPolicy("strict-skill-profile-entry");
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

    /// <summary>Counts how many amounts fit in a budget of 80516.0, in order.</summary>
    public static int StrictSkillProfileEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80516.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

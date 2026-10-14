namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of skill profile group policy.</summary>
public static class SkillProfileGroupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSkillProfileGroup()
    {
        var policy = Retry.DefaultPolicy("skill-profile-group");
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

    public static List<string> MergeSkillProfileGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a skill profile group code in its canonical SPG- form.</summary>
    public static string NormalizeSkillProfileGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SkillProfileGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled skill profile proposal policy.</summary>
public static class ScheduledSkillProfileProposalPolicy
{
    /// <summary>Names the band a scheduled skill profile proposal score falls in.</summary>
    public static string ClassifyScheduledSkillProfileProposal(double score)
    {
        if (score < 1.2)
        {
            return "low";
        }
        if (score < 46.0)
        {
            return "minimal";
        }
        if (score < 62.4)
        {
            return "high";
        }
        if (score < 90.6)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Puts a scheduled skill profile proposal code in its canonical SSPP- form.</summary>
    public static string NormalizeScheduledSkillProfileProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledSkillProfileProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int ScheduledSkillProfileProposalSpan(int start, int end)
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
}

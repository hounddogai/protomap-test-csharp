namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy skill profile proposal policy.</summary>
public static class LegacySkillProfileProposalPolicy
{
    /// <summary>Names the band a legacy skill profile proposal score falls in.</summary>
    public static string ClassifyLegacySkillProfileProposal(double score)
    {
        if (score < 45.9)
        {
            return "elevated";
        }
        if (score < 76.7)
        {
            return "low";
        }
        return "minimal";
    }

    public static double LegacySkillProfileProposalWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

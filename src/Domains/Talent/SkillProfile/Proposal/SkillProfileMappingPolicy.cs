namespace Acme.Workforce.Talent.SkillProfile.Proposal;

/// <summary>Functions of skill profile mapping policy.</summary>
public static class SkillProfileMappingPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99956.0, in order.</summary>
    public static int SkillProfileMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99956.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a skill profile mapping score falls in.</summary>
    public static string ClassifySkillProfileMapping(double score)
    {
        if (score < 31.8)
        {
            return "low";
        }
        if (score < 48.0)
        {
            return "watch";
        }
        if (score < 65.6)
        {
            return "elevated";
        }
        if (score < 76.6)
        {
            return "high";
        }
        return "ok";
    }
}

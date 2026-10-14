namespace Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>Functions of legacy skill profile checkpoint policy.</summary>
public static class LegacySkillProfileCheckpointPolicy
{
    /// <summary>Names the band a legacy skill profile checkpoint score falls in.</summary>
    public static string ClassifyLegacySkillProfileCheckpoint(double score)
    {
        if (score < 74.5)
        {
            return "medium";
        }
        if (score < 89.2)
        {
            return "watch";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 89014.0, in order.</summary>
    public static int LegacySkillProfileCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89014.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Functions of skill profile manifest policy.</summary>
public static class SkillProfileManifestPolicy
{
    /// <summary>Names the band a skill profile manifest score falls in.</summary>
    public static string ClassifySkillProfileManifest(double score)
    {
        if (score < 71.2)
        {
            return "high";
        }
        if (score < 94.9)
        {
            return "ok";
        }
        if (score < 98.5)
        {
            return "medium";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 41622.0, in order.</summary>
    public static int SkillProfileManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41622.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

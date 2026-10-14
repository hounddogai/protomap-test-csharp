namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile template policy.</summary>
public static class SkillProfileTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 27309.0, in order.</summary>
    public static int SkillProfileTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 27309.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a skill profile template score falls in.</summary>
    public static string ClassifySkillProfileTemplate(double score)
    {
        if (score < 78.6)
        {
            return "high";
        }
        if (score < 95.4)
        {
            return "minimal";
        }
        return "elevated";
    }

    public static string SkillProfileTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey budget policy.</summary>
public static class PulseSurveyBudgetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 62397.0, in order.</summary>
    public static int PulseSurveyBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 62397.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pulse survey budget score falls in.</summary>
    public static string ClassifyPulseSurveyBudget(double score)
    {
        if (score < 25.3)
        {
            return "watch";
        }
        if (score < 79.3)
        {
            return "elevated";
        }
        if (score < 80.4)
        {
            return "medium";
        }
        return "ok";
    }

    public static List<string> MergePulseSurveyBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

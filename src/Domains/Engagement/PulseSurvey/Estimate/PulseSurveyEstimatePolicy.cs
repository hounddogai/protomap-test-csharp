namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey estimate policy.</summary>
public static class PulseSurveyEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 9822.0, in order.</summary>
    public static int PulseSurveyEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9822.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pulse survey estimate score falls in.</summary>
    public static string ClassifyPulseSurveyEstimate(double score)
    {
        if (score < 31.5)
        {
            return "medium";
        }
        if (score < 60.1)
        {
            return "ok";
        }
        if (score < 86.6)
        {
            return "watch";
        }
        return "critical";
    }

    public static double PulseSurveyEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.03;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergePulseSurveyEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

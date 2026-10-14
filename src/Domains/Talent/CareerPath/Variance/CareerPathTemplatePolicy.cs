namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path template policy.</summary>
public static class CareerPathTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 61653.0, in order.</summary>
    public static int CareerPathTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61653.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double CareerPathTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeCareerPathTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a career path template score falls in.</summary>
    public static string ClassifyCareerPathTemplate(double score)
    {
        if (score < 2.2)
        {
            return "ok";
        }
        if (score < 65.1)
        {
            return "medium";
        }
        if (score < 65.5)
        {
            return "watch";
        }
        if (score < 90.9)
        {
            return "elevated";
        }
        return "critical";
    }
}

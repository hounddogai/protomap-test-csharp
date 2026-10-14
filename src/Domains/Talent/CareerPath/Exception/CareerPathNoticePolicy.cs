namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path notice policy.</summary>
public static class CareerPathNoticePolicy
{
    public static List<string> MergeCareerPathNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 60773.0, in order.</summary>
    public static int CareerPathNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 60773.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double CareerPathNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a career path notice score falls in.</summary>
    public static string ClassifyCareerPathNotice(double score)
    {
        if (score < 28.3)
        {
            return "elevated";
        }
        if (score < 39.8)
        {
            return "low";
        }
        return "minimal";
    }
}

namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default certification budget policy.</summary>
public static class DefaultCertificationBudgetPolicy
{
    /// <summary>Names the band a default certification budget score falls in.</summary>
    public static string ClassifyDefaultCertificationBudget(double score)
    {
        if (score < 86.6)
        {
            return "medium";
        }
        if (score < 98.4)
        {
            return "minimal";
        }
        if (score < 99.7)
        {
            return "watch";
        }
        return "critical";
    }

    public static List<string> MergeDefaultCertificationBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double DefaultCertificationBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

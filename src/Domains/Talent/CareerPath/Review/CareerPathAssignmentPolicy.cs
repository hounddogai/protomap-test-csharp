namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path assignment policy.</summary>
public static class CareerPathAssignmentPolicy
{
    public static List<string> MergeCareerPathAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a career path assignment score falls in.</summary>
    public static string ClassifyCareerPathAssignment(double score)
    {
        if (score < 40.6)
        {
            return "critical";
        }
        if (score < 44.3)
        {
            return "high";
        }
        return "watch";
    }

    public static double CareerPathAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

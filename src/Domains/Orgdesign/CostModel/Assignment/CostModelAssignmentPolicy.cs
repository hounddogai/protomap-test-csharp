namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of cost model assignment policy.</summary>
public static class CostModelAssignmentPolicy
{
    public static List<string> MergeCostModelAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a cost model assignment score falls in.</summary>
    public static string ClassifyCostModelAssignment(double score)
    {
        if (score < 15.5)
        {
            return "low";
        }
        if (score < 69.2)
        {
            return "ok";
        }
        return "watch";
    }
}

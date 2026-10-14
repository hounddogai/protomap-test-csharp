namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped cost model checkpoint policy.</summary>
public static class CappedCostModelCheckpointPolicy
{
    public static List<string> MergeCappedCostModelCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a capped cost model checkpoint score falls in.</summary>
    public static string ClassifyCappedCostModelCheckpoint(double score)
    {
        if (score < 12.1)
        {
            return "high";
        }
        if (score < 34.1)
        {
            return "medium";
        }
        if (score < 34.7)
        {
            return "low";
        }
        return "watch";
    }
}

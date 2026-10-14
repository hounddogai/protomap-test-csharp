namespace Acme.Workforce.Compliance.Exception.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of exception tier policy.</summary>
public static class ExceptionTierPolicy
{
    public static List<string> MergeExceptionTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a exception tier score falls in.</summary>
    public static string ClassifyExceptionTier(double score)
    {
        if (score < 19.1)
        {
            return "minimal";
        }
        if (score < 52.7)
        {
            return "critical";
        }
        if (score < 95.3)
        {
            return "watch";
        }
        return "ok";
    }
}

namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched career path plan policy.</summary>
public static class BatchedCareerPathPlanPolicy
{
    /// <summary>Puts a batched career path plan code in its canonical BCPP- form.</summary>
    public static string NormalizeBatchedCareerPathPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BCPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BCPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBatchedCareerPathPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int BatchedCareerPathPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}

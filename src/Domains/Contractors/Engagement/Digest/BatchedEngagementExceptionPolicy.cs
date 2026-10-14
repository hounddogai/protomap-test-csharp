namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched engagement exception policy.</summary>
public static class BatchedEngagementExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 52833.0, in order.</summary>
    public static int BatchedEngagementExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52833.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched engagement exception code in its canonical BEE- form.</summary>
    public static string NormalizeBatchedEngagementExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BEE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BEE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeBatchedEngagementExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double BatchedEngagementExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

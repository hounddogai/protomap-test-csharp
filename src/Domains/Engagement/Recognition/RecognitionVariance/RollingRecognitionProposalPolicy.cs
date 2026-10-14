namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling recognition proposal policy.</summary>
public static class RollingRecognitionProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 77475.0, in order.</summary>
    public static int RollingRecognitionProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77475.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeRollingRecognitionProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a rolling recognition proposal code in its canonical RRP- form.</summary>
    public static string NormalizeRollingRecognitionProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

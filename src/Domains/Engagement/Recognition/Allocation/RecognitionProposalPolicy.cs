namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of recognition proposal policy.</summary>
public static class RecognitionProposalPolicy
{
    /// <summary>Names the band a recognition proposal score falls in.</summary>
    public static string ClassifyRecognitionProposal(double score)
    {
        if (score < 4.5)
        {
            return "high";
        }
        if (score < 93.2)
        {
            return "minimal";
        }
        return "watch";
    }

    /// <summary>Puts a recognition proposal code in its canonical RP- form.</summary>
    public static string NormalizeRecognitionProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeRecognitionProposalLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 12243.0, in order.</summary>
    public static int RecognitionProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12243.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

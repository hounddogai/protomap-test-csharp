namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of training due agreement policy.</summary>
public static class TrainingDueAgreementPolicy
{
    /// <summary>Names the band a training due agreement score falls in.</summary>
    public static string ClassifyTrainingDueAgreement(double score)
    {
        if (score < 31.5)
        {
            return "low";
        }
        if (score < 90.2)
        {
            return "watch";
        }
        return "critical";
    }

    public static List<string> MergeTrainingDueAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a training due agreement code in its canonical TDA- form.</summary>
    public static string NormalizeTrainingDueAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TDA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TDA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

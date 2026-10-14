namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment agreement policy.</summary>
public static class AssignmentAgreementPolicy
{
    /// <summary>Puts a assignment agreement code in its canonical AA- form.</summary>
    public static string NormalizeAssignmentAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAssignmentAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string AssignmentAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a assignment agreement score falls in.</summary>
    public static string ClassifyAssignmentAgreement(double score)
    {
        if (score < 35.5)
        {
            return "elevated";
        }
        if (score < 63.5)
        {
            return "low";
        }
        if (score < 76.7)
        {
            return "critical";
        }
        if (score < 77.9)
        {
            return "ok";
        }
        return "minimal";
    }
}

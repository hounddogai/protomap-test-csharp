namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of exception agreement policy.</summary>
public static class ExceptionAgreementPolicy
{
    /// <summary>Puts a exception agreement code in its canonical EA- form.</summary>
    public static string NormalizeExceptionAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a exception agreement score falls in.</summary>
    public static string ClassifyExceptionAgreement(double score)
    {
        if (score < 4.3)
        {
            return "ok";
        }
        if (score < 10.5)
        {
            return "medium";
        }
        if (score < 38.9)
        {
            return "low";
        }
        if (score < 93.6)
        {
            return "high";
        }
        return "elevated";
    }

    public static string ExceptionAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeExceptionAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

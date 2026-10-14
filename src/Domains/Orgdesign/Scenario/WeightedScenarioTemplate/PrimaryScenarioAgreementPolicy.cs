namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary scenario agreement policy.</summary>
public static class PrimaryScenarioAgreementPolicy
{
    public static List<string> MergePrimaryScenarioAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int PrimaryScenarioAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static string PrimaryScenarioAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a primary scenario agreement code in its canonical PSA- form.</summary>
    public static string NormalizePrimaryScenarioAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

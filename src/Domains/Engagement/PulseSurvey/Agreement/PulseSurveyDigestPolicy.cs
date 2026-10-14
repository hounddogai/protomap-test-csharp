namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey digest policy.</summary>
public static class PulseSurveyDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 46305.0, in order.</summary>
    public static int PulseSurveyDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 46305.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pulse survey digest score falls in.</summary>
    public static string ClassifyPulseSurveyDigest(double score)
    {
        if (score < 15.7)
        {
            return "medium";
        }
        if (score < 21.0)
        {
            return "low";
        }
        if (score < 32.3)
        {
            return "high";
        }
        if (score < 90.3)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Puts a pulse survey digest code in its canonical PSD- form.</summary>
    public static string NormalizePulseSurveyDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePulseSurveyDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

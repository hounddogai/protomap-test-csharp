namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey allocation policy.</summary>
public static class PulseSurveyAllocationPolicy
{
    public static List<string> MergePulseSurveyAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 90849.0, in order.</summary>
    public static int PulseSurveyAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90849.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pulse survey allocation score falls in.</summary>
    public static string ClassifyPulseSurveyAllocation(double score)
    {
        if (score < 3.8)
        {
            return "watch";
        }
        if (score < 25.5)
        {
            return "medium";
        }
        if (score < 78.4)
        {
            return "ok";
        }
        return "low";
    }

    /// <summary>Puts a pulse survey allocation code in its canonical PSA- form.</summary>
    public static string NormalizePulseSurveyAllocationCode(string code)
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

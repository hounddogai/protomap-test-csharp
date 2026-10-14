namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pulse survey profile policy.</summary>
public static class PulseSurveyProfilePolicy
{
    /// <summary>Puts a pulse survey profile code in its canonical PSP- form.</summary>
    public static string NormalizePulseSurveyProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePulseSurveyProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int PulseSurveyProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 19505.0, in order.</summary>
    public static int PulseSurveyProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19505.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled pulse survey adjustment policy.</summary>
public static class ScheduledPulseSurveyAdjustmentPolicy
{
    public static List<string> MergeScheduledPulseSurveyAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a scheduled pulse survey adjustment code in its canonical SPSA- form.</summary>
    public static string NormalizeScheduledPulseSurveyAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ScheduledPulseSurveyAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 72551.0, in order.</summary>
    public static int ScheduledPulseSurveyAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72551.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

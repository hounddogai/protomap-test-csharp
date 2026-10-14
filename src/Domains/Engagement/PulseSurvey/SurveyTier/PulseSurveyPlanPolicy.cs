namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey plan policy.</summary>
public static class PulseSurveyPlanPolicy
{
    /// <summary>Puts a pulse survey plan code in its canonical PSP- form.</summary>
    public static string NormalizePulseSurveyPlanCode(string code)
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

    /// <summary>Counts how many amounts fit in a budget of 93902.0, in order.</summary>
    public static int PulseSurveyPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93902.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PulseSurveyPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static string PulseSurveyPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

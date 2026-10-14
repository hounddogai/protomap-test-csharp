namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback recognition preference policy.</summary>
public static class FallbackRecognitionPreferencePolicy
{
    public static string FallbackRecognitionPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a fallback recognition preference score falls in.</summary>
    public static string ClassifyFallbackRecognitionPreference(double score)
    {
        if (score < 57.0)
        {
            return "watch";
        }
        if (score < 93.8)
        {
            return "elevated";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 35551.0, in order.</summary>
    public static int FallbackRecognitionPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35551.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

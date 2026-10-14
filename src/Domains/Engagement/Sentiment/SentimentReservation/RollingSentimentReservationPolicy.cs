namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>Functions of rolling sentiment reservation policy.</summary>
public static class RollingSentimentReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 54721.0, in order.</summary>
    public static int RollingSentimentReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54721.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a rolling sentiment reservation code in its canonical RSR- form.</summary>
    public static string NormalizeRollingSentimentReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey reservation policy.</summary>
public static class PulseSurveyReservationPolicy
{
    /// <summary>Names the band a pulse survey reservation score falls in.</summary>
    public static string ClassifyPulseSurveyReservation(double score)
    {
        if (score < 40.7)
        {
            return "low";
        }
        if (score < 59.0)
        {
            return "critical";
        }
        if (score < 99.0)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 42796.0, in order.</summary>
    public static int PulseSurveyReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42796.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a pulse survey reservation code in its canonical PSR- form.</summary>
    public static string NormalizePulseSurveyReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PulseSurveyReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.2;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

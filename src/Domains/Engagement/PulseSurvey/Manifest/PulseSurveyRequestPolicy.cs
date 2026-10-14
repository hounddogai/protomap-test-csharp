namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pulse survey request policy.</summary>
public static class PulseSurveyRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePulseSurveyRequest()
    {
        var policy = Retry.DefaultPolicy("pulse-survey-request");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 25668.0, in order.</summary>
    public static int PulseSurveyRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25668.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PulseSurveyRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a pulse survey request code in its canonical PSR- form.</summary>
    public static string NormalizePulseSurveyRequestCode(string code)
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
}

namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pulse survey milestone policy.</summary>
public static class PulseSurveyMilestonePolicy
{
    public static string PulseSurveyMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePulseSurveyMilestone()
    {
        var policy = Retry.DefaultPolicy("pulse-survey-milestone");
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

    /// <summary>Puts a pulse survey milestone code in its canonical PSM- form.</summary>
    public static string NormalizePulseSurveyMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 5866.0, in order.</summary>
    public static int PulseSurveyMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5866.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pulse survey notice policy.</summary>
public static class PulseSurveyNoticePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 1370.0, in order.</summary>
    public static int PulseSurveyNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1370.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PulseSurveyNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pulse survey notice score falls in.</summary>
    public static string ClassifyPulseSurveyNotice(double score)
    {
        if (score < 4.1)
        {
            return "medium";
        }
        if (score < 27.2)
        {
            return "elevated";
        }
        if (score < 31.1)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePulseSurveyNotice()
    {
        var policy = Retry.DefaultPolicy("pulse-survey-notice");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}

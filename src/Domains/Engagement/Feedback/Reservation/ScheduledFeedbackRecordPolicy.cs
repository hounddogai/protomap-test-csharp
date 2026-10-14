namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled feedback record policy.</summary>
public static class ScheduledFeedbackRecordPolicy
{
    /// <summary>Puts a scheduled feedback record code in its canonical SFR- form.</summary>
    public static string NormalizeScheduledFeedbackRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ScheduledFeedbackRecordSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 39711.0, in order.</summary>
    public static int ScheduledFeedbackRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39711.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double ScheduledFeedbackRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

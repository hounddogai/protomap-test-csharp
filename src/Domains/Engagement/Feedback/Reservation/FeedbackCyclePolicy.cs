namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback cycle policy.</summary>
public static class FeedbackCyclePolicy
{
    public static string FeedbackCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 63408.0, in order.</summary>
    public static int FeedbackCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63408.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

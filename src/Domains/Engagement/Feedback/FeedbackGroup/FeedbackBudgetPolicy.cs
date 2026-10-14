namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback budget policy.</summary>
public static class FeedbackBudgetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 20254.0, in order.</summary>
    public static int FeedbackBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20254.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string FeedbackBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}

namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary feedback claim policy.</summary>
public static class PrimaryFeedbackClaimPolicy
{
    public static double PrimaryFeedbackClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.91;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 86538.0, in order.</summary>
    public static int PrimaryFeedbackClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86538.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

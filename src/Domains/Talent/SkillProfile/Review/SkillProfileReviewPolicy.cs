namespace Acme.Workforce.Talent.SkillProfile.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of skill profile review policy.</summary>
public static class SkillProfileReviewPolicy
{
    public static string SkillProfileReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double SkillProfileReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.45;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 99836.0, in order.</summary>
    public static int SkillProfileReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99836.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Talent.SkillProfile.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default skill profile segment policy.</summary>
public static class DefaultSkillProfileSegmentPolicy
{
    /// <summary>Names the band a default skill profile segment score falls in.</summary>
    public static string ClassifyDefaultSkillProfileSegment(double score)
    {
        if (score < 2.0)
        {
            return "low";
        }
        if (score < 3.4)
        {
            return "elevated";
        }
        if (score < 13.3)
        {
            return "medium";
        }
        if (score < 57.3)
        {
            return "minimal";
        }
        return "critical";
    }

    public static int DefaultSkillProfileSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 80738.0, in order.</summary>
    public static int DefaultSkillProfileSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80738.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string DefaultSkillProfileSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}

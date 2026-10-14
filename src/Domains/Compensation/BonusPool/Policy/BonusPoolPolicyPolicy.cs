namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool policy policy.</summary>
public static class BonusPoolPolicyPolicy
{
    public static List<string> MergeBonusPoolPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string BonusPoolPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a bonus pool policy score falls in.</summary>
    public static string ClassifyBonusPoolPolicy(double score)
    {
        if (score < 38.7)
        {
            return "elevated";
        }
        if (score < 67.5)
        {
            return "medium";
        }
        return "minimal";
    }

    public static int BonusPoolPolicySpan(int start, int end)
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
        return span / 60;
    }
}

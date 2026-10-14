namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool reservation policy.</summary>
public static class BonusPoolReservationPolicy
{
    public static double BonusPoolReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeBonusPoolReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a bonus pool reservation score falls in.</summary>
    public static string ClassifyBonusPoolReservation(double score)
    {
        if (score < 5.4)
        {
            return "high";
        }
        if (score < 35.6)
        {
            return "minimal";
        }
        return "critical";
    }
}

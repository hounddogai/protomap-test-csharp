namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict accrual slot policy.</summary>
public static class StrictAccrualSlotPolicy
{
    public static List<string> MergeStrictAccrualSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a strict accrual slot score falls in.</summary>
    public static string ClassifyStrictAccrualSlot(double score)
    {
        if (score < 2.3)
        {
            return "high";
        }
        if (score < 43.9)
        {
            return "ok";
        }
        if (score < 90.2)
        {
            return "medium";
        }
        return "elevated";
    }

    public static double StrictAccrualSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

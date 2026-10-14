namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped merit cycle score policy.</summary>
public static class CappedMeritCycleScorePolicy
{
    public static List<string> MergeCappedMeritCycleScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double CappedMeritCycleScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

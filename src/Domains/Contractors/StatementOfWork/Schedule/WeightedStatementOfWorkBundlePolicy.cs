namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted statement of work bundle policy.</summary>
public static class WeightedStatementOfWorkBundlePolicy
{
    public static List<string> MergeWeightedStatementOfWorkBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double WeightedStatementOfWorkBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

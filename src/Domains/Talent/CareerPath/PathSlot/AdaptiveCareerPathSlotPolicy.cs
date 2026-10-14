namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive career path slot policy.</summary>
public static class AdaptiveCareerPathSlotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 85342.0, in order.</summary>
    public static int AdaptiveCareerPathSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85342.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAdaptiveCareerPathSlotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

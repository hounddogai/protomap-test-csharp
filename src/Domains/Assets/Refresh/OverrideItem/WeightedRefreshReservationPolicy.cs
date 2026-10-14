namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted refresh reservation policy.</summary>
public static class WeightedRefreshReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 83270.0, in order.</summary>
    public static int WeightedRefreshReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 83270.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a weighted refresh reservation code in its canonical WRR- form.</summary>
    public static string NormalizeWeightedRefreshReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double WeightedRefreshReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeWeightedRefreshReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

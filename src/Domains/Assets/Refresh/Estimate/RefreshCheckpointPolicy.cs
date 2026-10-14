namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh checkpoint policy.</summary>
public static class RefreshCheckpointPolicy
{
    /// <summary>Puts a refresh checkpoint code in its canonical RC- form.</summary>
    public static string NormalizeRefreshCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RefreshCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeRefreshCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 9358.0, in order.</summary>
    public static int RefreshCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9358.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

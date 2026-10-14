namespace Acme.Workforce.Workplace.Occupancy.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of occupancy preference policy.</summary>
public static class OccupancyPreferencePolicy
{
    /// <summary>Puts a occupancy preference code in its canonical OP- form.</summary>
    public static string NormalizeOccupancyPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeOccupancyPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 56527.0, in order.</summary>
    public static int OccupancyPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56527.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyPreference()
    {
        var policy = Retry.DefaultPolicy("occupancy-preference");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}

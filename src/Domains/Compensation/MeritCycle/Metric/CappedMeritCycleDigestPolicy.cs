namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped merit cycle digest policy.</summary>
public static class CappedMeritCycleDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 98112.0, in order.</summary>
    public static int CappedMeritCycleDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98112.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CappedMeritCycleDigestSpan(int start, int end)
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
        return span / 3600;
    }

    public static List<string> MergeCappedMeritCycleDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a capped merit cycle digest code in its canonical CMCD- form.</summary>
    public static string NormalizeCappedMeritCycleDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMCD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMCD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

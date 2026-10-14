namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default headcount target policy.</summary>
public static class DefaultHeadcountTargetPolicy
{
    /// <summary>Puts a default headcount target code in its canonical DHT- form.</summary>
    public static string NormalizeDefaultHeadcountTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DHT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DHT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int DefaultHeadcountTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 22760.0, in order.</summary>
    public static int DefaultHeadcountTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22760.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeDefaultHeadcountTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

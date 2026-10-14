namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default exception digest policy.</summary>
public static class DefaultExceptionDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72699.0, in order.</summary>
    public static int DefaultExceptionDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72699.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int DefaultExceptionDigestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static List<string> MergeDefaultExceptionDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a default exception digest code in its canonical DED- form.</summary>
    public static string NormalizeDefaultExceptionDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DED-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DED-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of nomination request policy.</summary>
public static class NominationRequestPolicy
{
    /// <summary>Puts a nomination request code in its canonical NR- form.</summary>
    public static string NormalizeNominationRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int NominationRequestSpan(int start, int end)
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
        return span / 60;
    }

    public static List<string> MergeNominationRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 13721.0, in order.</summary>
    public static int NominationRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13721.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

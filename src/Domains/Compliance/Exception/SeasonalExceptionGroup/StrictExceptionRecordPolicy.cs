namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict exception record policy.</summary>
public static class StrictExceptionRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 63308.0, in order.</summary>
    public static int StrictExceptionRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63308.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a strict exception record code in its canonical SER- form.</summary>
    public static string NormalizeStrictExceptionRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SER-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SER-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeStrictExceptionRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string StrictExceptionRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}

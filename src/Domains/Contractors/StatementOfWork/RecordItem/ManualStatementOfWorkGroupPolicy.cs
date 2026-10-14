namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual statement of work group policy.</summary>
public static class ManualStatementOfWorkGroupPolicy
{
    public static string ManualStatementOfWorkGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a manual statement of work group code in its canonical MSOWG- form.</summary>
    public static string NormalizeManualStatementOfWorkGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MSOWG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MSOWG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeManualStatementOfWorkGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 80623.0, in order.</summary>
    public static int ManualStatementOfWorkGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80623.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

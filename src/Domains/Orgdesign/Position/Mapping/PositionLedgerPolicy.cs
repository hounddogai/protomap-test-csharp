namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of position ledger policy.</summary>
public static class PositionLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 76365.0, in order.</summary>
    public static int PositionLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 76365.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergePositionLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a position ledger code in its canonical PL- form.</summary>
    public static string NormalizePositionLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PositionLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}

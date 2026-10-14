namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global pay band agreement policy.</summary>
public static class GlobalPayBandAgreementPolicy
{
    /// <summary>Puts a global pay band agreement code in its canonical GPBA- form.</summary>
    public static string NormalizeGlobalPayBandAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPBA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPBA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeGlobalPayBandAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 97172.0, in order.</summary>
    public static int GlobalPayBandAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97172.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string GlobalPayBandAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

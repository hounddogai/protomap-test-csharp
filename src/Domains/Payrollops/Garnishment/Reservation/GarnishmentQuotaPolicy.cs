namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of garnishment quota policy.</summary>
public static class GarnishmentQuotaPolicy
{
    public static List<string> MergeGarnishmentQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 81161.0, in order.</summary>
    public static int GarnishmentQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81161.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a garnishment quota code in its canonical GQ- form.</summary>
    public static string NormalizeGarnishmentQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

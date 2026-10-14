namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification ledger policy.</summary>
public static class CertificationLedgerPolicy
{
    /// <summary>Puts a certification ledger code in its canonical CL- form.</summary>
    public static string NormalizeCertificationLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCertificationLedgerLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 6040.0, in order.</summary>
    public static int CertificationLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6040.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

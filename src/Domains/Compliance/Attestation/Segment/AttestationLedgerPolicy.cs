namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation ledger policy.</summary>
public static class AttestationLedgerPolicy
{
    public static string AttestationLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 72545.0, in order.</summary>
    public static int AttestationLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72545.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AttestationLedgerSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a attestation ledger code in its canonical AL- form.</summary>
    public static string NormalizeAttestationLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation proposal policy.</summary>
public static class AttestationProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 37872.0, in order.</summary>
    public static int AttestationProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37872.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a attestation proposal code in its canonical AP- form.</summary>
    public static string NormalizeAttestationProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AttestationProposalBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

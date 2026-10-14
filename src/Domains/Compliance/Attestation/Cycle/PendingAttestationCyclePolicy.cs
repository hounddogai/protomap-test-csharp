namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending attestation cycle policy.</summary>
public static class PendingAttestationCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 56298.0, in order.</summary>
    public static int PendingAttestationCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56298.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pending attestation cycle score falls in.</summary>
    public static string ClassifyPendingAttestationCycle(double score)
    {
        if (score < 7.2)
        {
            return "low";
        }
        if (score < 40.4)
        {
            return "high";
        }
        if (score < 45.5)
        {
            return "elevated";
        }
        if (score < 73.9)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Puts a pending attestation cycle code in its canonical PAC- form.</summary>
    public static string NormalizePendingAttestationCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePendingAttestationCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

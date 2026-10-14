namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of attestation draft policy.</summary>
public static class AttestationDraftPolicy
{
    /// <summary>Puts a attestation draft code in its canonical AD- form.</summary>
    public static string NormalizeAttestationDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAttestationDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAttestationDraft()
    {
        var policy = Retry.DefaultPolicy("attestation-draft");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 54859.0, in order.</summary>
    public static int AttestationDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54859.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

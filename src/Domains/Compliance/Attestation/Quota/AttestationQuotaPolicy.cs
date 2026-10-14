namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of attestation quota policy.</summary>
public static class AttestationQuotaPolicy
{
    public static int AttestationQuotaSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    public static List<string> MergeAttestationQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 10589.0, in order.</summary>
    public static int AttestationQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10589.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a attestation quota score falls in.</summary>
    public static string ClassifyAttestationQuota(double score)
    {
        if (score < 51.9)
        {
            return "critical";
        }
        if (score < 55.1)
        {
            return "elevated";
        }
        if (score < 64.4)
        {
            return "watch";
        }
        if (score < 65.8)
        {
            return "low";
        }
        return "medium";
    }
}

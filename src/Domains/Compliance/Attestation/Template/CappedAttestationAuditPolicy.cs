namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped attestation audit policy.</summary>
public static class CappedAttestationAuditPolicy
{
    public static string CappedAttestationAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeCappedAttestationAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a capped attestation audit score falls in.</summary>
    public static string ClassifyCappedAttestationAudit(double score)
    {
        if (score < 33.1)
        {
            return "ok";
        }
        if (score < 39.1)
        {
            return "high";
        }
        if (score < 93.1)
        {
            return "critical";
        }
        return "elevated";
    }

    public static int CappedAttestationAuditSpan(int start, int end)
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
        return span / 60;
    }
}

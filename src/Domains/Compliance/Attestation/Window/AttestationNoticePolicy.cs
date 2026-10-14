namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of attestation notice policy.</summary>
public static class AttestationNoticePolicy
{
    public static int AttestationNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a attestation notice score falls in.</summary>
    public static string ClassifyAttestationNotice(double score)
    {
        if (score < 14.6)
        {
            return "watch";
        }
        if (score < 34.7)
        {
            return "ok";
        }
        return "high";
    }

    public static List<string> MergeAttestationNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a attestation notice code in its canonical AN- form.</summary>
    public static string NormalizeAttestationNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

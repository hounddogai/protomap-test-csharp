namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification assignment policy.</summary>
public static class CertificationAssignmentPolicy
{
    public static List<string> MergeCertificationAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int CertificationAssignmentSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a certification assignment code in its canonical CA- form.</summary>
    public static string NormalizeCertificationAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

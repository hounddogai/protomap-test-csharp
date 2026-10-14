namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification review policy.</summary>
public static class CertificationReviewPolicy
{
    public static List<string> MergeCertificationReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a certification review score falls in.</summary>
    public static string ClassifyCertificationReview(double score)
    {
        if (score < 4.5)
        {
            return "medium";
        }
        if (score < 33.3)
        {
            return "ok";
        }
        if (score < 75.6)
        {
            return "critical";
        }
        if (score < 75.6)
        {
            return "high";
        }
        return "watch";
    }

    public static int CertificationReviewSpan(int start, int end)
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

    /// <summary>Puts a certification review code in its canonical CR- form.</summary>
    public static string NormalizeCertificationReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

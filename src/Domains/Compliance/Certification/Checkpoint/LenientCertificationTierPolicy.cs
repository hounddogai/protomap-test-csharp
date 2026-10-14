namespace Acme.Workforce.Compliance.Certification.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient certification tier policy.</summary>
public static class LenientCertificationTierPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientCertificationTier()
    {
        var policy = Retry.DefaultPolicy("lenient-certification-tier");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a lenient certification tier code in its canonical LCT- form.</summary>
    public static string NormalizeLenientCertificationTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LCT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LCT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLenientCertificationTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}

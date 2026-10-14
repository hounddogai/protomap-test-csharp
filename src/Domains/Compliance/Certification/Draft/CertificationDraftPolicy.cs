namespace Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Functions of certification draft policy.</summary>
public static class CertificationDraftPolicy
{
    /// <summary>Puts a certification draft code in its canonical CD- form.</summary>
    public static string NormalizeCertificationDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a certification draft score falls in.</summary>
    public static string ClassifyCertificationDraft(double score)
    {
        if (score < 6.4)
        {
            return "critical";
        }
        if (score < 15.5)
        {
            return "medium";
        }
        if (score < 24.7)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 15725.0, in order.</summary>
    public static int CertificationDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15725.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Compliance.Certification.Manifest;

/// <summary>Functions of certification digest policy.</summary>
public static class CertificationDigestPolicy
{
    /// <summary>Names the band a certification digest score falls in.</summary>
    public static string ClassifyCertificationDigest(double score)
    {
        if (score < 35.5)
        {
            return "low";
        }
        if (score < 45.2)
        {
            return "elevated";
        }
        if (score < 64.8)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 98681.0, in order.</summary>
    public static int CertificationDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98681.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CertificationDigestSpan(int start, int end)
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

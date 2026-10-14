namespace Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Functions of visitor agreement policy.</summary>
public static class VisitorAgreementPolicy
{
    public static int VisitorAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a visitor agreement score falls in.</summary>
    public static string ClassifyVisitorAgreement(double score)
    {
        if (score < 7.7)
        {
            return "elevated";
        }
        if (score < 89.6)
        {
            return "watch";
        }
        if (score < 95.4)
        {
            return "minimal";
        }
        return "ok";
    }
}

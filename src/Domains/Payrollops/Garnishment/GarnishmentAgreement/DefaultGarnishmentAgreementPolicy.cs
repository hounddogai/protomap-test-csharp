namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

/// <summary>Functions of default garnishment agreement policy.</summary>
public static class DefaultGarnishmentAgreementPolicy
{
    public static int DefaultGarnishmentAgreementSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a default garnishment agreement score falls in.</summary>
    public static string ClassifyDefaultGarnishmentAgreement(double score)
    {
        if (score < 8.7)
        {
            return "high";
        }
        if (score < 16.4)
        {
            return "medium";
        }
        return "ok";
    }
}

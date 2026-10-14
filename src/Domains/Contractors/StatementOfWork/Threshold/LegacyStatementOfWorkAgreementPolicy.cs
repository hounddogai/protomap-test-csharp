namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Functions of legacy statement of work agreement policy.</summary>
public static class LegacyStatementOfWorkAgreementPolicy
{
    public static int LegacyStatementOfWorkAgreementSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a legacy statement of work agreement score falls in.</summary>
    public static string ClassifyLegacyStatementOfWorkAgreement(double score)
    {
        if (score < 20.0)
        {
            return "minimal";
        }
        if (score < 83.8)
        {
            return "elevated";
        }
        return "high";
    }
}

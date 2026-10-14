namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary leave request agreement policy.</summary>
public static class PrimaryLeaveRequestAgreementPolicy
{
    /// <summary>Puts a primary leave request agreement code in its canonical PLRA- form.</summary>
    public static string NormalizePrimaryLeaveRequestAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PLRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PLRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a primary leave request agreement score falls in.</summary>
    public static string ClassifyPrimaryLeaveRequestAgreement(double score)
    {
        if (score < 10.1)
        {
            return "medium";
        }
        if (score < 40.2)
        {
            return "watch";
        }
        if (score < 62.8)
        {
            return "minimal";
        }
        return "ok";
    }

    public static int PrimaryLeaveRequestAgreementSpan(int start, int end)
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

    public static string PrimaryLeaveRequestAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}

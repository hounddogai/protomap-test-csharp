namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled statement of work record policy.</summary>
public static class ScheduledStatementOfWorkRecordPolicy
{
    /// <summary>Puts a scheduled statement of work record code in its canonical SSOWR- form.</summary>
    public static string NormalizeScheduledStatementOfWorkRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSOWR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSOWR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ScheduledStatementOfWorkRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a scheduled statement of work record score falls in.</summary>
    public static string ClassifyScheduledStatementOfWorkRecord(double score)
    {
        if (score < 18.3)
        {
            return "watch";
        }
        if (score < 27.3)
        {
            return "minimal";
        }
        if (score < 81.5)
        {
            return "medium";
        }
        if (score < 87.4)
        {
            return "critical";
        }
        return "low";
    }
}

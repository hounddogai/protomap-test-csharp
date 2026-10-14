namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of statement of work batch policy.</summary>
public static class StatementOfWorkBatchPolicy
{
    /// <summary>Puts a statement of work batch code in its canonical SOWB- form.</summary>
    public static string NormalizeStatementOfWorkBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string StatementOfWorkBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a statement of work batch score falls in.</summary>
    public static string ClassifyStatementOfWorkBatch(double score)
    {
        if (score < 2.4)
        {
            return "ok";
        }
        if (score < 63.3)
        {
            return "low";
        }
        if (score < 82.1)
        {
            return "critical";
        }
        if (score < 86.5)
        {
            return "elevated";
        }
        return "medium";
    }
}

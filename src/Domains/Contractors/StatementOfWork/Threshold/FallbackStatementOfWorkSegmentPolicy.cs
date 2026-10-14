namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback statement of work segment policy.</summary>
public static class FallbackStatementOfWorkSegmentPolicy
{
    public static string FallbackStatementOfWorkSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a fallback statement of work segment code in its canonical FSOWS- form.</summary>
    public static string NormalizeFallbackStatementOfWorkSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FSOWS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FSOWS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

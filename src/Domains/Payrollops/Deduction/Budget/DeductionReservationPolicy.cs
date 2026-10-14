namespace Acme.Workforce.Payrollops.Deduction.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of deduction reservation policy.</summary>
public static class DeductionReservationPolicy
{
    /// <summary>Puts a deduction reservation code in its canonical DR- form.</summary>
    public static string NormalizeDeductionReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a deduction reservation score falls in.</summary>
    public static string ClassifyDeductionReservation(double score)
    {
        if (score < 35.6)
        {
            return "low";
        }
        if (score < 68.6)
        {
            return "high";
        }
        return "medium";
    }

    public static string DeductionReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}

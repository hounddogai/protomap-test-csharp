namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict accrual exception policy.</summary>
public static class StrictAccrualExceptionPolicy
{
    public static string StrictAccrualExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a strict accrual exception code in its canonical SAE- form.</summary>
    public static string NormalizeStrictAccrualExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

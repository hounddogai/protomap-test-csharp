namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual accrual digest policy.</summary>
public static class ManualAccrualDigestPolicy
{
    public static double ManualAccrualDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a manual accrual digest code in its canonical MAD- form.</summary>
    public static string NormalizeManualAccrualDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MAD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MAD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

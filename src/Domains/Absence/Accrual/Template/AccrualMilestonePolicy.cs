namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual milestone policy.</summary>
public static class AccrualMilestonePolicy
{
    public static double AccrualMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a accrual milestone code in its canonical AM- form.</summary>
    public static string NormalizeAccrualMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

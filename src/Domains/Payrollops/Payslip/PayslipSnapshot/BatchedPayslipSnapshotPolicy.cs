namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched payslip snapshot policy.</summary>
public static class BatchedPayslipSnapshotPolicy
{
    public static double BatchedPayslipSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a batched payslip snapshot code in its canonical BPS- form.</summary>
    public static string NormalizeBatchedPayslipSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 67610.0, in order.</summary>
    public static int BatchedPayslipSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67610.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of holiday batch policy.</summary>
public static class HolidayBatchPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHolidayBatch()
    {
        var policy = Retry.DefaultPolicy("holiday-batch");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a holiday batch code in its canonical HB- form.</summary>
    public static string NormalizeHolidayBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double HolidayBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}

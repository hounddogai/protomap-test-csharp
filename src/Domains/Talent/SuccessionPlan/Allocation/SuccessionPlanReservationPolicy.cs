namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of succession plan reservation policy.</summary>
public static class SuccessionPlanReservationPolicy
{
    public static int SuccessionPlanReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a succession plan reservation code in its canonical SPR- form.</summary>
    public static string NormalizeSuccessionPlanReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SuccessionPlanReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSuccessionPlanReservation()
    {
        var policy = Retry.DefaultPolicy("succession-plan-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}

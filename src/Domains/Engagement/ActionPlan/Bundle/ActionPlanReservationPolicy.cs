namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of action plan reservation policy.</summary>
public static class ActionPlanReservationPolicy
{
    /// <summary>Puts a action plan reservation code in its canonical APR- form.</summary>
    public static string NormalizeActionPlanReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ActionPlanReservationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeActionPlanReservation()
    {
        var policy = Retry.DefaultPolicy("action-plan-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}

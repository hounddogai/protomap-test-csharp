namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped scenario reservation policy.</summary>
public static class CappedScenarioReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedScenarioReservation()
    {
        var policy = Retry.DefaultPolicy("capped-scenario-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int CappedScenarioReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a capped scenario reservation code in its canonical CSR- form.</summary>
    public static string NormalizeCappedScenarioReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

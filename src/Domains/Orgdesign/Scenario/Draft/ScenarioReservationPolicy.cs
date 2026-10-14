namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scenario reservation policy.</summary>
public static class ScenarioReservationPolicy
{
    /// <summary>Puts a scenario reservation code in its canonical SR- form.</summary>
    public static string NormalizeScenarioReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a scenario reservation score falls in.</summary>
    public static string ClassifyScenarioReservation(double score)
    {
        if (score < 28.0)
        {
            return "critical";
        }
        if (score < 35.4)
        {
            return "medium";
        }
        if (score < 98.5)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScenarioReservation()
    {
        var policy = Retry.DefaultPolicy("scenario-reservation");
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

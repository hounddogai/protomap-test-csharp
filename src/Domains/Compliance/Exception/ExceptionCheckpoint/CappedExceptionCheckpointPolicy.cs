namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>Functions of capped exception checkpoint policy.</summary>
public static class CappedExceptionCheckpointPolicy
{
    /// <summary>Names the band a capped exception checkpoint score falls in.</summary>
    public static string ClassifyCappedExceptionCheckpoint(double score)
    {
        if (score < 43.3)
        {
            return "ok";
        }
        if (score < 47.0)
        {
            return "critical";
        }
        if (score < 48.7)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Puts a capped exception checkpoint code in its canonical CEC- form.</summary>
    public static string NormalizeCappedExceptionCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CEC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CEC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

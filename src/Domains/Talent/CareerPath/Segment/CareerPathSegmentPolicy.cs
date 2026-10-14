namespace Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Functions of career path segment policy.</summary>
public static class CareerPathSegmentPolicy
{
    /// <summary>Names the band a career path segment score falls in.</summary>
    public static string ClassifyCareerPathSegment(double score)
    {
        if (score < 1.0)
        {
            return "critical";
        }
        if (score < 10.1)
        {
            return "minimal";
        }
        return "ok";
    }

    /// <summary>Puts a career path segment code in its canonical CPS- form.</summary>
    public static string NormalizeCareerPathSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched skill profile forecast policy.</summary>
public static class BatchedSkillProfileForecastPolicy
{
    public static int BatchedSkillProfileForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedSkillProfileForecast()
    {
        var policy = Retry.DefaultPolicy("batched-skill-profile-forecast");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a batched skill profile forecast score falls in.</summary>
    public static string ClassifyBatchedSkillProfileForecast(double score)
    {
        if (score < 70.8)
        {
            return "low";
        }
        if (score < 88.6)
        {
            return "critical";
        }
        return "minimal";
    }

    /// <summary>Puts a batched skill profile forecast code in its canonical BSPF- form.</summary>
    public static string NormalizeBatchedSkillProfileForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BSPF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BSPF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

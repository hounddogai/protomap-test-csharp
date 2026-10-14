namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of laptop request policy.</summary>
public static class LaptopRequestPolicy
{
    /// <summary>Names the band a laptop request score falls in.</summary>
    public static string ClassifyLaptopRequest(double score)
    {
        if (score < 8.6)
        {
            return "critical";
        }
        if (score < 27.4)
        {
            return "minimal";
        }
        if (score < 74.0)
        {
            return "low";
        }
        if (score < 75.9)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLaptopRequest()
    {
        var policy = Retry.DefaultPolicy("laptop-request");
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

    /// <summary>Puts a laptop request code in its canonical LR- form.</summary>
    public static string NormalizeLaptopRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

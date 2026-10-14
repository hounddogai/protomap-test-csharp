namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched laptop variance policy.</summary>
public static class BatchedLaptopVariancePolicy
{
    public static int BatchedLaptopVarianceSpan(int start, int end)
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

    /// <summary>Names the band a batched laptop variance score falls in.</summary>
    public static string ClassifyBatchedLaptopVariance(double score)
    {
        if (score < 4.8)
        {
            return "medium";
        }
        if (score < 16.0)
        {
            return "minimal";
        }
        if (score < 47.8)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedLaptopVariance()
    {
        var policy = Retry.DefaultPolicy("batched-laptop-variance");
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

    /// <summary>Puts a batched laptop variance code in its canonical BLV- form.</summary>
    public static string NormalizeBatchedLaptopVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BLV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BLV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched position segment policy.</summary>
public static class BatchedPositionSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedPositionSegment()
    {
        var policy = Retry.DefaultPolicy("batched-position-segment");
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

    /// <summary>Counts how many amounts fit in a budget of 93467.0, in order.</summary>
    public static int BatchedPositionSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93467.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched position segment code in its canonical BPS- form.</summary>
    public static string NormalizeBatchedPositionSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a batched position segment score falls in.</summary>
    public static string ClassifyBatchedPositionSegment(double score)
    {
        if (score < 80.6)
        {
            return "watch";
        }
        if (score < 94.4)
        {
            return "elevated";
        }
        return "minimal";
    }
}

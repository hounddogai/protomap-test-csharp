namespace Acme.Workforce.Assets.Warranty.Assignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of warranty assignment policy.</summary>
public static class WarrantyAssignmentPolicy
{
    /// <summary>Names the band a warranty assignment score falls in.</summary>
    public static string ClassifyWarrantyAssignment(double score)
    {
        if (score < 17.2)
        {
            return "critical";
        }
        if (score < 33.5)
        {
            return "watch";
        }
        if (score < 42.0)
        {
            return "high";
        }
        if (score < 49.2)
        {
            return "ok";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWarrantyAssignment()
    {
        var policy = Retry.DefaultPolicy("warranty-assignment");
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

    public static string WarrantyAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a warranty assignment code in its canonical WA- form.</summary>
    public static string NormalizeWarrantyAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

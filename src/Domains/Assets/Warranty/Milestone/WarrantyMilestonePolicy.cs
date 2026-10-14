namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of warranty milestone policy.</summary>
public static class WarrantyMilestonePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWarrantyMilestone()
    {
        var policy = Retry.DefaultPolicy("warranty-milestone");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string WarrantyMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a warranty milestone code in its canonical WM- form.</summary>
    public static string NormalizeWarrantyMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending laptop target policy.</summary>
public static class PendingLaptopTargetPolicy
{
    public static string PendingLaptopTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a pending laptop target code in its canonical PLT- form.</summary>
    public static string NormalizePendingLaptopTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PLT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PLT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingLaptopTarget()
    {
        var policy = Retry.DefaultPolicy("pending-laptop-target");
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
}

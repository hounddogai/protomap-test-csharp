namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of visitor target policy.</summary>
public static class VisitorTargetPolicy
{
    /// <summary>Puts a visitor target code in its canonical VT- form.</summary>
    public static string NormalizeVisitorTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string VisitorTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorTarget()
    {
        var policy = Retry.DefaultPolicy("visitor-target");
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
}

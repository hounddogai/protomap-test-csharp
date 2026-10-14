namespace Acme.Workforce.Compliance.Exception.Cycle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of exception cycle policy.</summary>
public static class ExceptionCyclePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeExceptionCycle()
    {
        var policy = Retry.DefaultPolicy("exception-cycle");
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

    public static string ExceptionCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a exception cycle code in its canonical EC- form.</summary>
    public static string NormalizeExceptionCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

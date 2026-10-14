namespace Acme.Workforce.Assets.Laptop.Mapping;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default laptop window policy.</summary>
public static class DefaultLaptopWindowPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultLaptopWindow()
    {
        var policy = Retry.DefaultPolicy("default-laptop-window");
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

    public static string DefaultLaptopWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int DefaultLaptopWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a default laptop window code in its canonical DLW- form.</summary>
    public static string NormalizeDefaultLaptopWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DLW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DLW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

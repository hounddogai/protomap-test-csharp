namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of engagement slot policy.</summary>
public static class EngagementSlotPolicy
{
    public static string EngagementSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a engagement slot code in its canonical ES- form.</summary>
    public static string NormalizeEngagementSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ES-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ES-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEngagementSlot()
    {
        var policy = Retry.DefaultPolicy("engagement-slot");
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

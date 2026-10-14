namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of career path slot policy.</summary>
public static class CareerPathSlotPolicy
{
    /// <summary>Puts a career path slot code in its canonical CPS- form.</summary>
    public static string NormalizeCareerPathSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathSlot()
    {
        var policy = Retry.DefaultPolicy("career-path-slot");
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
}

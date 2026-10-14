namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending visitor slot policy.</summary>
public static class PendingVisitorSlotPolicy
{
    /// <summary>Puts a pending visitor slot code in its canonical PVS- form.</summary>
    public static string NormalizePendingVisitorSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PVS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PVS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingVisitorSlot()
    {
        var policy = Retry.DefaultPolicy("pending-visitor-slot");
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
}

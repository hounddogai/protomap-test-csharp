namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending warranty assignment policy.</summary>
public static class PendingWarrantyAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingWarrantyAssignment()
    {
        var policy = Retry.DefaultPolicy("pending-warranty-assignment");
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

    /// <summary>Puts a pending warranty assignment code in its canonical PWA- form.</summary>
    public static string NormalizePendingWarrantyAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PWA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PWA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}

namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped vendor assignment policy.</summary>
public static class CappedVendorAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedVendorAssignment()
    {
        var policy = Retry.DefaultPolicy("capped-vendor-assignment");
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

    /// <summary>Names the band a capped vendor assignment score falls in.</summary>
    public static string ClassifyCappedVendorAssignment(double score)
    {
        if (score < 4.5)
        {
            return "elevated";
        }
        if (score < 48.4)
        {
            return "ok";
        }
        if (score < 68.5)
        {
            return "minimal";
        }
        if (score < 75.8)
        {
            return "high";
        }
        return "medium";
    }
}

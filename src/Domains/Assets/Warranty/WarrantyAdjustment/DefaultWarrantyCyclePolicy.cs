namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default warranty cycle policy.</summary>
public static class DefaultWarrantyCyclePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultWarrantyCycle()
    {
        var policy = Retry.DefaultPolicy("default-warranty-cycle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int DefaultWarrantyCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}

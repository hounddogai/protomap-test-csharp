namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending warranty allocation policy.</summary>
public static class PendingWarrantyAllocationPolicy
{
    public static string PendingWarrantyAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingWarrantyAllocation()
    {
        var policy = Retry.DefaultPolicy("pending-warranty-allocation");
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

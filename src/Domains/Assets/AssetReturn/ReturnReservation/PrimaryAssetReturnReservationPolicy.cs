namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary asset return reservation policy.</summary>
public static class PrimaryAssetReturnReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryAssetReturnReservation()
    {
        var policy = Retry.DefaultPolicy("primary-asset-return-reservation");
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

    /// <summary>Counts how many amounts fit in a budget of 2463.0, in order.</summary>
    public static int PrimaryAssetReturnReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 2463.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

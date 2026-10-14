namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy asset return draft policy.</summary>
public static class LegacyAssetReturnDraftPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyAssetReturnDraft()
    {
        var policy = Retry.DefaultPolicy("legacy-asset-return-draft");
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

    /// <summary>Counts how many amounts fit in a budget of 70983.0, in order.</summary>
    public static int LegacyAssetReturnDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 70983.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}

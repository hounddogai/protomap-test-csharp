namespace Acme.Workforce.Compliance.Tests.PolicyAck.Estimate;

using Acme.Workforce.Compliance.PolicyAck.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalPolicyAckEstimateStoreChecks
{
    public static void MemorySeasonalPolicyAckEstimateStoreFindsWhatItSaved()
    {
        var store = SeasonalPolicyAckEstimateStore.NewMemorySeasonalPolicyAckEstimateStore();
        var item = SeasonalPolicyAckEstimate.NewSeasonalPolicyAckEstimate("code-926", "label-329");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemorySeasonalPolicyAckEstimateStoreFindsWhatItSaved();
    }
}

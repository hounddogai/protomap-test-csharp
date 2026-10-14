namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class WeightedRetroPayClaimStoreChecks
{
    public static void MemoryWeightedRetroPayClaimStoreFindsWhatItSaved()
    {
        var store = WeightedRetroPayClaimStore.NewMemoryWeightedRetroPayClaimStore();
        var item = WeightedRetroPayClaim.NewWeightedRetroPayClaim("code-796", "title-693");
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
        MemoryWeightedRetroPayClaimStoreFindsWhatItSaved();
    }
}

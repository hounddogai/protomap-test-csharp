namespace Acme.Workforce.Payrollops.Tests.RetroPay.Cycle;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class SeasonalRetroPayClaimStoreChecks
{
    public static void MemorySeasonalRetroPayClaimStoreFindsWhatItSaved()
    {
        var store = SeasonalRetroPayClaimStore.NewMemorySeasonalRetroPayClaimStore();
        var item = SeasonalRetroPayClaim.NewSeasonalRetroPayClaim("code-530", "name-8");
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
        MemorySeasonalRetroPayClaimStoreFindsWhatItSaved();
    }
}

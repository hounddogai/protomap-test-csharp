namespace Acme.Workforce.Compensation.Tests.MeritCycle.Claim;

using Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class FallbackMeritCycleClaimStoreChecks
{
    public static void MemoryFallbackMeritCycleClaimStoreFindsWhatItSaved()
    {
        var store = FallbackMeritCycleClaimStore.NewMemoryFallbackMeritCycleClaimStore();
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("code-427", "label-592");
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
        MemoryFallbackMeritCycleClaimStoreFindsWhatItSaved();
    }
}

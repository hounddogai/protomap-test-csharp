namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayBandPolicyStoreChecks
{
    public static void MemoryPayBandPolicyStoreFindsWhatItSaved()
    {
        var store = PayBandPolicyStore.NewMemoryPayBandPolicyStore();
        var item = PayBandPolicy.NewPayBandPolicy("code-608", "title-306");
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
        MemoryPayBandPolicyStoreFindsWhatItSaved();
    }
}

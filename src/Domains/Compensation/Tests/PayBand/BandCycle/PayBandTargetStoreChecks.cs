namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandTargetStoreChecks
{
    public static void MemoryPayBandTargetStoreFindsWhatItSaved()
    {
        var store = PayBandTargetStore.NewMemoryPayBandTargetStore();
        var item = PayBandTarget.NewPayBandTarget("code-551", "name-611");
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
        MemoryPayBandTargetStoreFindsWhatItSaved();
    }
}

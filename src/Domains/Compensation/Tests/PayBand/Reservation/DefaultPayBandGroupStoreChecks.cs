namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultPayBandGroupStoreChecks
{
    public static void MemoryDefaultPayBandGroupStoreFindsWhatItSaved()
    {
        var store = DefaultPayBandGroupStore.NewMemoryDefaultPayBandGroupStore();
        var item = DefaultPayBandGroup.NewDefaultPayBandGroup("code-336", "title-491");
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
        MemoryDefaultPayBandGroupStoreFindsWhatItSaved();
    }
}

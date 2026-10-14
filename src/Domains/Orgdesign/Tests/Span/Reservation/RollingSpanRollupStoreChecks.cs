namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RollingSpanRollupStoreChecks
{
    public static void MemoryRollingSpanRollupStoreFindsWhatItSaved()
    {
        var store = RollingSpanRollupStore.NewMemoryRollingSpanRollupStore();
        var item = RollingSpanRollup.NewRollingSpanRollup("code-122", "name-453");
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
        MemoryRollingSpanRollupStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentProfileStoreChecks
{
    public static void MemoryGarnishmentProfileStoreFindsWhatItSaved()
    {
        var store = GarnishmentProfileStore.NewMemoryGarnishmentProfileStore();
        var item = GarnishmentProfile.NewGarnishmentProfile("code-423", "title-546");
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
        MemoryGarnishmentProfileStoreFindsWhatItSaved();
    }
}

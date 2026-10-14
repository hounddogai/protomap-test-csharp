namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GarnishmentTargetStoreChecks
{
    public static void MemoryGarnishmentTargetStoreFindsWhatItSaved()
    {
        var store = GarnishmentTargetStore.NewMemoryGarnishmentTargetStore();
        var item = GarnishmentTarget.NewGarnishmentTarget("code-867", "title-78");
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
        MemoryGarnishmentTargetStoreFindsWhatItSaved();
    }
}

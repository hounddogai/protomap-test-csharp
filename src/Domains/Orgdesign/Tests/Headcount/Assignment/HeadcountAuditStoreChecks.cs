namespace Acme.Workforce.Orgdesign.Tests.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class HeadcountAuditStoreChecks
{
    public static void MemoryHeadcountAuditStoreFindsWhatItSaved()
    {
        var store = HeadcountAuditStore.NewMemoryHeadcountAuditStore();
        var item = HeadcountAudit.NewHeadcountAudit("code-764", "label-232");
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
        MemoryHeadcountAuditStoreFindsWhatItSaved();
    }
}

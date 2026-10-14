namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyNoticeStoreChecks
{
    public static void MemoryPolicyNoticeStoreFindsWhatItSaved()
    {
        var store = PolicyNoticeStore.NewMemoryPolicyNoticeStore();
        var item = PolicyNotice.NewPolicyNotice("code-229", "name-318");
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
        MemoryPolicyNoticeStoreFindsWhatItSaved();
    }
}

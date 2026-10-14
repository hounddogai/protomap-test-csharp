namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PoolNoticeStoreChecks
{
    public static void MemoryPoolNoticeStoreFindsWhatItSaved()
    {
        var store = PoolNoticeStore.NewMemoryPoolNoticeStore();
        var item = PoolNotice.NewPoolNotice("code-407", "label-999");
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
        MemoryPoolNoticeStoreFindsWhatItSaved();
    }
}

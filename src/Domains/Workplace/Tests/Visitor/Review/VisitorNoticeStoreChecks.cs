namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VisitorNoticeStoreChecks
{
    public static void MemoryVisitorNoticeStoreFindsWhatItSaved()
    {
        var store = VisitorNoticeStore.NewMemoryVisitorNoticeStore();
        var item = VisitorNotice.NewVisitorNotice("code-770", "name-864");
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
        MemoryVisitorNoticeStoreFindsWhatItSaved();
    }
}

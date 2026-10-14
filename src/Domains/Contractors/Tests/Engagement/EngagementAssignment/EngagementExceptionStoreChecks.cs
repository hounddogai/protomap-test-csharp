namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class EngagementExceptionStoreChecks
{
    public static void MemoryEngagementExceptionStoreFindsWhatItSaved()
    {
        var store = EngagementExceptionStore.NewMemoryEngagementExceptionStore();
        var item = EngagementException.NewEngagementException("code-305", "title-668");
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
        MemoryEngagementExceptionStoreFindsWhatItSaved();
    }
}

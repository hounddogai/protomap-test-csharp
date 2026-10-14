namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentEntryStoreChecks
{
    public static void MemoryAssignmentEntryStoreFindsWhatItSaved()
    {
        var store = AssignmentEntryStore.NewMemoryAssignmentEntryStore();
        var item = AssignmentEntry.NewAssignmentEntry("code-555", "name-182");
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
        MemoryAssignmentEntryStoreFindsWhatItSaved();
    }
}

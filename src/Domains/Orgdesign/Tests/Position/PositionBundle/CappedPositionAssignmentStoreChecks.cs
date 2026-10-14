namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class CappedPositionAssignmentStoreChecks
{
    public static void MemoryCappedPositionAssignmentStoreFindsWhatItSaved()
    {
        var store = CappedPositionAssignmentStore.NewMemoryCappedPositionAssignmentStore();
        var item = CappedPositionAssignment.NewCappedPositionAssignment("code-379", "name-69");
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
        MemoryCappedPositionAssignmentStoreFindsWhatItSaved();
    }
}

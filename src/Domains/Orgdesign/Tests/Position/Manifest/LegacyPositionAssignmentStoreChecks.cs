namespace Acme.Workforce.Orgdesign.Tests.Position.Manifest;

using Acme.Workforce.Orgdesign.Position.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LegacyPositionAssignmentStoreChecks
{
    public static void MemoryLegacyPositionAssignmentStoreFindsWhatItSaved()
    {
        var store = LegacyPositionAssignmentStore.NewMemoryLegacyPositionAssignmentStore();
        var item = LegacyPositionAssignment.NewLegacyPositionAssignment("code-352", "title-542");
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
        MemoryLegacyPositionAssignmentStoreFindsWhatItSaved();
    }
}

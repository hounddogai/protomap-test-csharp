namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyTownHallAssignmentStoreChecks
{
    public static void MemoryLegacyTownHallAssignmentStoreFindsWhatItSaved()
    {
        var store = LegacyTownHallAssignmentStore.NewMemoryLegacyTownHallAssignmentStore();
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("code-254", "label-490");
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
        MemoryLegacyTownHallAssignmentStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class MobilityAssignmentStoreChecks
{
    public static void MemoryMobilityAssignmentStoreFindsWhatItSaved()
    {
        var store = MobilityAssignmentStore.NewMemoryMobilityAssignmentStore();
        var item = MobilityAssignment.NewMobilityAssignment("code-254", "label-78");
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
        MemoryMobilityAssignmentStoreFindsWhatItSaved();
    }
}

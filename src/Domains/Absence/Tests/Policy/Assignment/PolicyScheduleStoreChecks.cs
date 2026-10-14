namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyScheduleStoreChecks
{
    public static void MemoryPolicyScheduleStoreFindsWhatItSaved()
    {
        var store = PolicyScheduleStore.NewMemoryPolicyScheduleStore();
        var item = PolicySchedule.NewPolicySchedule("code-932", "title-618");
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
        MemoryPolicyScheduleStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Compliance.Tests.PolicyAck.Schedule;

using Acme.Workforce.Compliance.PolicyAck.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredPolicyAckScheduleStoreChecks
{
    public static void MemoryTieredPolicyAckScheduleStoreFindsWhatItSaved()
    {
        var store = TieredPolicyAckScheduleStore.NewMemoryTieredPolicyAckScheduleStore();
        var item = TieredPolicyAckSchedule.NewTieredPolicyAckSchedule("code-46", "title-319");
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
        MemoryTieredPolicyAckScheduleStoreFindsWhatItSaved();
    }
}

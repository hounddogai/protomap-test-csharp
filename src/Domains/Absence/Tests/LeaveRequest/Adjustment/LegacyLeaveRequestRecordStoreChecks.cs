namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyLeaveRequestRecordStoreChecks
{
    public static void MemoryLegacyLeaveRequestRecordStoreFindsWhatItSaved()
    {
        var store = LegacyLeaveRequestRecordStore.NewMemoryLegacyLeaveRequestRecordStore();
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("code-592", "name-332");
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
        MemoryLegacyLeaveRequestRecordStoreFindsWhatItSaved();
    }
}

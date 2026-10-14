namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PositionAuditStoreChecks
{
    public static void MemoryPositionAuditStoreFindsWhatItSaved()
    {
        var store = PositionAuditStore.NewMemoryPositionAuditStore();
        var item = PositionAudit.NewPositionAudit("code-569", "title-765");
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
        MemoryPositionAuditStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckGroup;

using Acme.Workforce.Compliance.PolicyAck.AckGroup;

/// <summary>Checks of ack group code, run by the project's runner.</summary>
public static class StrictPolicyAckTemplateStoreChecks
{
    public static void MemoryStrictPolicyAckTemplateStoreFindsWhatItSaved()
    {
        var store = StrictPolicyAckTemplateStore.NewMemoryStrictPolicyAckTemplateStore();
        var item = StrictPolicyAckTemplate.NewStrictPolicyAckTemplate("code-586", "label-604");
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
        MemoryStrictPolicyAckTemplateStoreFindsWhatItSaved();
    }
}

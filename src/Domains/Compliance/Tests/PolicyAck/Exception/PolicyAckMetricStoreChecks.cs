namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PolicyAckMetricStoreChecks
{
    public static void MemoryPolicyAckMetricStoreFindsWhatItSaved()
    {
        var store = PolicyAckMetricStore.NewMemoryPolicyAckMetricStore();
        var item = PolicyAckMetric.NewPolicyAckMetric("code-432", "label-840");
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
        MemoryPolicyAckMetricStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationRuleStoreChecks
{
    public static void MemoryAttestationRuleStoreFindsWhatItSaved()
    {
        var store = AttestationRuleStore.NewMemoryAttestationRuleStore();
        var item = AttestationRule.NewAttestationRule("code-179", "title-420");
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
        MemoryAttestationRuleStoreFindsWhatItSaved();
    }
}

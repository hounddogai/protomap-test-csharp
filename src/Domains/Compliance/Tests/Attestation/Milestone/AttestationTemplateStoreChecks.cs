namespace Acme.Workforce.Compliance.Tests.Attestation.Milestone;

using Acme.Workforce.Compliance.Attestation.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AttestationTemplateStoreChecks
{
    public static void MemoryAttestationTemplateStoreFindsWhatItSaved()
    {
        var store = AttestationTemplateStore.NewMemoryAttestationTemplateStore();
        var item = AttestationTemplate.NewAttestationTemplate("code-293", "title-275");
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
        MemoryAttestationTemplateStoreFindsWhatItSaved();
    }
}

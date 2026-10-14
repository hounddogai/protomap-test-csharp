namespace Acme.Workforce.Compliance.Tests.Certification.Draft;

using Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CertificationScoreStoreChecks
{
    public static void MemoryCertificationScoreStoreFindsWhatItSaved()
    {
        var store = CertificationScoreStore.NewMemoryCertificationScoreStore();
        var item = CertificationScore.NewCertificationScore("code-371", "label-440");
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
        MemoryCertificationScoreStoreFindsWhatItSaved();
    }
}

namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LenientExceptionVarianceStoreChecks
{
    public static void MemoryLenientExceptionVarianceStoreFindsWhatItSaved()
    {
        var store = LenientExceptionVarianceStore.NewMemoryLenientExceptionVarianceStore();
        var item = LenientExceptionVariance.NewLenientExceptionVariance("code-37", "label-18");
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
        MemoryLenientExceptionVarianceStoreFindsWhatItSaved();
    }
}

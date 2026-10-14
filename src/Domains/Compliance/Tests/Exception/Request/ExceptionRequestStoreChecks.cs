namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionRequestStoreChecks
{
    public static void MemoryExceptionRequestStoreFindsWhatItSaved()
    {
        var store = ExceptionRequestStore.NewMemoryExceptionRequestStore();
        var item = ExceptionRequest.NewExceptionRequest("code-466", "label-110");
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
        MemoryExceptionRequestStoreFindsWhatItSaved();
    }
}

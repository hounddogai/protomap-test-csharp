namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SpanAssignmentStoreChecks
{
    public static void MemorySpanAssignmentStoreFindsWhatItSaved()
    {
        var store = SpanAssignmentStore.NewMemorySpanAssignmentStore();
        var item = SpanAssignment.NewSpanAssignment("code-320", "name-701");
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
        MemorySpanAssignmentStoreFindsWhatItSaved();
    }
}

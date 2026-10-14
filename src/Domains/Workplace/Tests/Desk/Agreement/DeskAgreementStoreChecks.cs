namespace Acme.Workforce.Workplace.Tests.Desk.Agreement;

using Acme.Workforce.Workplace.Desk.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeskAgreementStoreChecks
{
    public static void MemoryDeskAgreementStoreFindsWhatItSaved()
    {
        var store = DeskAgreementStore.NewMemoryDeskAgreementStore();
        var item = DeskAgreement.NewDeskAgreement("code-858", "label-47");
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
        MemoryDeskAgreementStoreFindsWhatItSaved();
    }
}

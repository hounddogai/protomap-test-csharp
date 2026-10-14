namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceAgreementStoreChecks
{
    public static void MemoryBalanceAgreementStoreFindsWhatItSaved()
    {
        var store = BalanceAgreementStore.NewMemoryBalanceAgreementStore();
        var item = BalanceAgreement.NewBalanceAgreement("code-766", "label-479");
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
        MemoryBalanceAgreementStoreFindsWhatItSaved();
    }
}

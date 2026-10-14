namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class StrictRetroPayProposalStoreChecks
{
    public static void MemoryStrictRetroPayProposalStoreFindsWhatItSaved()
    {
        var store = StrictRetroPayProposalStore.NewMemoryStrictRetroPayProposalStore();
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("code-586", "name-67");
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
        MemoryStrictRetroPayProposalStoreFindsWhatItSaved();
    }
}

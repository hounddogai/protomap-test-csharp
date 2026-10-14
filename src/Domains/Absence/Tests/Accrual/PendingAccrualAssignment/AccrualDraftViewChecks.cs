namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class AccrualDraftViewChecks
{
    public static void AccrualDraftViewShowsTheStatusInCapitals()
    {
        var item = AccrualDraft.NewAccrualDraft("code-437", "label-267");
        var view = AccrualDraftView.ToAccrualDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualDraftView.ToAccrualDraftViews(new List<AccrualDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualDraftViewShowsTheStatusInCapitals();
    }
}

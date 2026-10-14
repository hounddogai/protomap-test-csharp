namespace Acme.Workforce.Workplace.Tests.Desk.Agreement;

using Acme.Workforce.Workplace.Desk.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeskAgreementViewChecks
{
    public static void DeskAgreementViewShowsTheStatusInCapitals()
    {
        var item = DeskAgreement.NewDeskAgreement("code-858", "label-47");
        var view = DeskAgreementView.ToDeskAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskAgreementView.ToDeskAgreementViews(new List<DeskAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskAgreementViewShowsTheStatusInCapitals();
    }
}

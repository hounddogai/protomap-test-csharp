namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredStatementOfWorkAgreementViewChecks
{
    public static void TieredStatementOfWorkAgreementViewShowsTheStatusInCapitals()
    {
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("code-770", "name-554");
        var view = TieredStatementOfWorkAgreementView.ToTieredStatementOfWorkAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredStatementOfWorkAgreementView.ToTieredStatementOfWorkAgreementViews(new List<TieredStatementOfWorkAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredStatementOfWorkAgreementViewShowsTheStatusInCapitals();
    }
}

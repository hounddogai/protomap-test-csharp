namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkAgreementViewChecks
{
    public static void LegacyStatementOfWorkAgreementViewShowsTheStatusInCapitals()
    {
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("code-846", "title-86");
        var view = LegacyStatementOfWorkAgreementView.ToLegacyStatementOfWorkAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyStatementOfWorkAgreementView.ToLegacyStatementOfWorkAgreementViews(new List<LegacyStatementOfWorkAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkAgreementViewShowsTheStatusInCapitals();
    }
}

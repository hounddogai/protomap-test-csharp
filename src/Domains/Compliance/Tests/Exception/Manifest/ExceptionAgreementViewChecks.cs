namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionAgreementViewChecks
{
    public static void ExceptionAgreementViewShowsTheStatusInCapitals()
    {
        var item = ExceptionAgreement.NewExceptionAgreement("code-28", "title-808");
        var view = ExceptionAgreementView.ToExceptionAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ExceptionAgreementView.ToExceptionAgreementViews(new List<ExceptionAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ExceptionAgreementViewShowsTheStatusInCapitals();
    }
}

namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class LegacyExceptionAgreementViewChecks
{
    public static void LegacyExceptionAgreementViewShowsTheStatusInCapitals()
    {
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("code-202", "name-227");
        var view = LegacyExceptionAgreementView.ToLegacyExceptionAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyExceptionAgreementView.ToLegacyExceptionAgreementViews(new List<LegacyExceptionAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyExceptionAgreementViewShowsTheStatusInCapitals();
    }
}

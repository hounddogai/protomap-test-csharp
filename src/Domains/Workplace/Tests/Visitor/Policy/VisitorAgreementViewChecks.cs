namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorAgreementViewChecks
{
    public static void VisitorAgreementViewShowsTheStatusInCapitals()
    {
        var item = VisitorAgreement.NewVisitorAgreement("code-321", "name-823");
        var view = VisitorAgreementView.ToVisitorAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VisitorAgreementView.ToVisitorAgreementViews(new List<VisitorAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VisitorAgreementViewShowsTheStatusInCapitals();
    }
}

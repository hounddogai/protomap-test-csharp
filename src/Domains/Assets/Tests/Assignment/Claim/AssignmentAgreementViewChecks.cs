namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAgreementViewChecks
{
    public static void AssignmentAgreementViewShowsTheStatusInCapitals()
    {
        var item = AssignmentAgreement.NewAssignmentAgreement("code-324", "title-870");
        var view = AssignmentAgreementView.ToAssignmentAgreementView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssignmentAgreementView.ToAssignmentAgreementViews(new List<AssignmentAgreement> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssignmentAgreementViewShowsTheStatusInCapitals();
    }
}

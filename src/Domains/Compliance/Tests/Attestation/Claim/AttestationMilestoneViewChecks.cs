namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationMilestoneViewChecks
{
    public static void AttestationMilestoneViewShowsTheStatusInCapitals()
    {
        var item = AttestationMilestone.NewAttestationMilestone("code-202", "label-844");
        var view = AttestationMilestoneView.ToAttestationMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationMilestoneView.ToAttestationMilestoneViews(new List<AttestationMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationMilestoneViewShowsTheStatusInCapitals();
    }
}

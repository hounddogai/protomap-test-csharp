namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckReservation;

using Acme.Workforce.Compliance.PolicyAck.AckReservation;

/// <summary>Checks of ack reservation code, run by the project's runner.</summary>
public static class PrimaryPolicyAckDraftViewChecks
{
    public static void PrimaryPolicyAckDraftViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPolicyAckDraft.NewPrimaryPolicyAckDraft("code-778", "title-102");
        var view = PrimaryPolicyAckDraftView.ToPrimaryPolicyAckDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPolicyAckDraftView.ToPrimaryPolicyAckDraftViews(new List<PrimaryPolicyAckDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckDraftViewShowsTheStatusInCapitals();
    }
}

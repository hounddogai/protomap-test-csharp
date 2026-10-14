namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PendingAttestationCycleViewChecks
{
    public static void PendingAttestationCycleViewShowsTheStatusInCapitals()
    {
        var item = PendingAttestationCycle.NewPendingAttestationCycle("code-664", "name-938");
        var view = PendingAttestationCycleView.ToPendingAttestationCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingAttestationCycleView.ToPendingAttestationCycleViews(new List<PendingAttestationCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingAttestationCycleViewShowsTheStatusInCapitals();
    }
}

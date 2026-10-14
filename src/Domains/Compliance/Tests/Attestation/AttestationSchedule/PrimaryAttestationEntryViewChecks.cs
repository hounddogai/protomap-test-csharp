namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class PrimaryAttestationEntryViewChecks
{
    public static void PrimaryAttestationEntryViewShowsTheStatusInCapitals()
    {
        var item = PrimaryAttestationEntry.NewPrimaryAttestationEntry("code-243", "label-971");
        var view = PrimaryAttestationEntryView.ToPrimaryAttestationEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryAttestationEntryView.ToPrimaryAttestationEntryViews(new List<PrimaryAttestationEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryAttestationEntryViewShowsTheStatusInCapitals();
    }
}

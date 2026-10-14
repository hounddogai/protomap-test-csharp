namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationEntryViewChecks
{
    public static void AttestationEntryViewShowsTheStatusInCapitals()
    {
        var item = AttestationEntry.NewAttestationEntry("code-882", "label-308");
        var view = AttestationEntryView.ToAttestationEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationEntryView.ToAttestationEntryViews(new List<AttestationEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationEntryViewShowsTheStatusInCapitals();
    }
}

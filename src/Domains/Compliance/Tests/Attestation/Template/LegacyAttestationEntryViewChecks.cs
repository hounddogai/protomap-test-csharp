namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyAttestationEntryViewChecks
{
    public static void LegacyAttestationEntryViewShowsTheStatusInCapitals()
    {
        var item = LegacyAttestationEntry.NewLegacyAttestationEntry("code-377", "label-36");
        var view = LegacyAttestationEntryView.ToLegacyAttestationEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyAttestationEntryView.ToLegacyAttestationEntryViews(new List<LegacyAttestationEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyAttestationEntryViewShowsTheStatusInCapitals();
    }
}

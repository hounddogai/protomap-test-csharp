namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AuditPreferenceViewChecks
{
    public static void AuditPreferenceViewShowsTheStatusInCapitals()
    {
        var item = AuditPreference.NewAuditPreference("code-709", "label-617");
        var view = AuditPreferenceView.ToAuditPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditPreferenceView.ToAuditPreferenceViews(new List<AuditPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditPreferenceViewShowsTheStatusInCapitals();
    }
}

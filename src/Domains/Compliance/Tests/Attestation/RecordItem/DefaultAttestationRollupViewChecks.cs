namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DefaultAttestationRollupViewChecks
{
    public static void DefaultAttestationRollupViewShowsTheStatusInCapitals()
    {
        var item = DefaultAttestationRollup.NewDefaultAttestationRollup("code-914", "label-874");
        var view = DefaultAttestationRollupView.ToDefaultAttestationRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultAttestationRollupView.ToDefaultAttestationRollupViews(new List<DefaultAttestationRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultAttestationRollupViewShowsTheStatusInCapitals();
    }
}

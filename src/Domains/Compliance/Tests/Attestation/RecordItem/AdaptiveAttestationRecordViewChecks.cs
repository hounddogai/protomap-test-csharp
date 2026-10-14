namespace Acme.Workforce.Compliance.Tests.Attestation.RecordItem;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveAttestationRecordViewChecks
{
    public static void AdaptiveAttestationRecordViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveAttestationRecord.NewAdaptiveAttestationRecord("code-640", "label-295");
        var view = AdaptiveAttestationRecordView.ToAdaptiveAttestationRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveAttestationRecordView.ToAdaptiveAttestationRecordViews(new List<AdaptiveAttestationRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveAttestationRecordViewShowsTheStatusInCapitals();
    }
}

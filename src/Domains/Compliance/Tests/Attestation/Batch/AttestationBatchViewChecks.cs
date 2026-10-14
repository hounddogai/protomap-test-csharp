namespace Acme.Workforce.Compliance.Tests.Attestation.Batch;

using Acme.Workforce.Compliance.Attestation.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AttestationBatchViewChecks
{
    public static void AttestationBatchViewShowsTheStatusInCapitals()
    {
        var item = AttestationBatch.NewAttestationBatch("code-454", "name-300");
        var view = AttestationBatchView.ToAttestationBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AttestationBatchView.ToAttestationBatchViews(new List<AttestationBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AttestationBatchViewShowsTheStatusInCapitals();
    }
}

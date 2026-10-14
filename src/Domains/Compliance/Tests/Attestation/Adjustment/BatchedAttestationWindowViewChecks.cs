namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BatchedAttestationWindowViewChecks
{
    public static void BatchedAttestationWindowViewShowsTheStatusInCapitals()
    {
        var item = BatchedAttestationWindow.NewBatchedAttestationWindow("code-969", "label-833");
        var view = BatchedAttestationWindowView.ToBatchedAttestationWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAttestationWindowView.ToBatchedAttestationWindowViews(new List<BatchedAttestationWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAttestationWindowViewShowsTheStatusInCapitals();
    }
}

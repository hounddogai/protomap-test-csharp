namespace Acme.Workforce.Compliance.Tests.Certification.Notice;

using Acme.Workforce.Compliance.Certification.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WeightedCertificationBatchViewChecks
{
    public static void WeightedCertificationBatchViewShowsTheStatusInCapitals()
    {
        var item = WeightedCertificationBatch.NewWeightedCertificationBatch("code-963", "title-448");
        var view = WeightedCertificationBatchView.ToWeightedCertificationBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedCertificationBatchView.ToWeightedCertificationBatchViews(new List<WeightedCertificationBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedCertificationBatchViewShowsTheStatusInCapitals();
    }
}

namespace Acme.Workforce.Contractors.StatementOfWork.Metric;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Functions of default statement of work metric from template.</summary>
public static class DefaultStatementOfWorkMetricFromTemplate
{
    public static Dictionary<string, double> SummarizeBatchedVendorTemplatesForMetric(List<BatchedVendorTemplate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<DefaultStatementOfWorkMetric> DeriveDefaultStatementOfWorkMetricsFromTemplate(List<BatchedVendorTemplate> items)
    {
        var outValue = new List<DefaultStatementOfWorkMetric>();
        foreach (var item in items)
        {
            var derived = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectDefaultStatementOfWorkMetric(DefaultStatementOfWorkMetric item)
    {
        return PendingVendorManifestPolicy.PendingVendorManifestWeight(3, true);
    }
}

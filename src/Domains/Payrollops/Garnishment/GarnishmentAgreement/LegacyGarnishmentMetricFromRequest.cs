namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Functions of legacy garnishment metric from request.</summary>
public static class LegacyGarnishmentMetricFromRequest
{
    public static Dictionary<string, double> SummarizeDefaultPayRunRequestsForMetric(List<DefaultPayRunRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<LegacyGarnishmentMetric> DeriveLegacyGarnishmentMetricsFromRequest(List<DefaultPayRunRequest> items)
    {
        var outValue = new List<LegacyGarnishmentMetric>();
        foreach (var item in items)
        {
            var derived = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLegacyGarnishmentMetric(LegacyGarnishmentMetric item)
    {
        return DefaultPayRunRequestPolicy.DefaultPayRunRequestBucket(item.WeightedTotal());
    }
}

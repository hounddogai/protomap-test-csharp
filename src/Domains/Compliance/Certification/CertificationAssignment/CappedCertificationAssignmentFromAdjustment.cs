namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>Functions of capped certification assignment from adjustment.</summary>
public static class CappedCertificationAssignmentFromAdjustment
{
    public static Dictionary<string, double> SummarizeScheduledCertificationAdjustmentsForAssignment(List<ScheduledCertificationAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<CappedCertificationAssignment> DeriveCappedCertificationAssignmentsFromAdjustment(List<ScheduledCertificationAdjustment> items)
    {
        var outValue = new List<CappedCertificationAssignment>();
        foreach (var item in items)
        {
            var derived = CappedCertificationAssignment.NewCappedCertificationAssignment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileCappedCertificationAssignment(CappedCertificationAssignment item)
    {
        return CertificationForecastPolicy.CertificationForecastBucket(item.NetExposure());
    }
}

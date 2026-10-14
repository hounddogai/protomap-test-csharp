namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Functions of regional certification adjustment from window.</summary>
public static class RegionalCertificationAdjustmentFromWindow
{
    public static Dictionary<string, double> SummarizeDefaultTrainingDueWindowsForAdjustment(List<DefaultTrainingDueWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<RegionalCertificationAdjustment> DeriveRegionalCertificationAdjustmentsFromWindow(List<DefaultTrainingDueWindow> items)
    {
        var outValue = new List<RegionalCertificationAdjustment>();
        foreach (var item in items)
        {
            var derived = RegionalCertificationAdjustment.NewRegionalCertificationAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyRegionalCertificationAdjustmentExtra(RegionalCertificationAdjustment item)
    {
        return DefaultTrainingDueWindowPolicy.DefaultTrainingDueWindowBucket(item.ProjectedExposure());
    }
}

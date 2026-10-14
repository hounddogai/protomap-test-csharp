namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Functions of scheduled certification adjustment from ledger.</summary>
public static class ScheduledCertificationAdjustmentFromLedger
{
    public static Dictionary<string, double> SummarizeRollingTrainingDueLedgersForAdjustment(List<RollingTrainingDueLedger> items)
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

    public static List<ScheduledCertificationAdjustment> DeriveScheduledCertificationAdjustmentsFromLedger(List<RollingTrainingDueLedger> items)
    {
        var outValue = new List<ScheduledCertificationAdjustment>();
        foreach (var item in items)
        {
            var derived = ScheduledCertificationAdjustment.NewScheduledCertificationAdjustment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveScheduledCertificationAdjustment(ScheduledCertificationAdjustment item)
    {
        return ScheduledTrainingDueAuditPolicy.NormalizeScheduledTrainingDueAuditCode(item.Code);
    }
}

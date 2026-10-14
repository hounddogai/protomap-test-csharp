namespace Acme.Workforce.Compliance.Certification.Snapshot;

using Acme.Workforce.Compliance.TrainingDue.Window;

/// <summary>Functions of certification batch from cycle.</summary>
public static class CertificationBatchFromCycle
{
    public static Dictionary<string, double> SummarizeTrainingDueCyclesForBatch(List<TrainingDueCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<CertificationBatch> DeriveCertificationBatchesFromCycle(List<TrainingDueCycle> items)
    {
        var outValue = new List<CertificationBatch>();
        foreach (var item in items)
        {
            var derived = CertificationBatch.NewCertificationBatch(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveCertificationBatch(CertificationBatch item)
    {
        return TrainingDueCyclePolicy.ClassifyTrainingDueCycle(item.NetVolume());
    }
}

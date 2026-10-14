namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Functions of certification plan from group.</summary>
public static class CertificationPlanFromGroup
{
    public static Dictionary<string, double> SummarizeTrainingDueGroupsForPlan(List<TrainingDueGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<CertificationPlan> DeriveCertificationPlansFromGroup(List<TrainingDueGroup> items)
    {
        var outValue = new List<CertificationPlan>();
        foreach (var item in items)
        {
            var derived = CertificationPlan.NewCertificationPlan(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceCertificationPlan(CertificationPlan item)
    {
        return TrainingDueTargetPolicy.TrainingDueTargetBucket(item.AdjustedValue());
    }
}

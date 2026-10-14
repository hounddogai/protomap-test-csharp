namespace Acme.Workforce.Compliance.TrainingDue.Notice;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Functions of training due notice from override.</summary>
public static class TrainingDueNoticeFromOverride
{
    public static Dictionary<string, double> SummarizeCertificationOverridesForNotice(List<CertificationOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<TrainingDueNotice> DeriveTrainingDueNoticesFromOverride(List<CertificationOverride> items)
    {
        var outValue = new List<TrainingDueNotice>();
        foreach (var item in items)
        {
            var derived = TrainingDueNotice.NewTrainingDueNotice(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileTrainingDueNotice(TrainingDueNotice item)
    {
        return CertificationOverridePolicy.NormalizeCertificationOverrideCode(item.Code);
    }
}

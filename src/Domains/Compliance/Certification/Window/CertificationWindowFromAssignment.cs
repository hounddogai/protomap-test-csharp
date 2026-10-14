namespace Acme.Workforce.Compliance.Certification.Window;

using Acme.Workforce.Compliance.TrainingDue.Draft;

/// <summary>Functions of certification window from assignment.</summary>
public static class CertificationWindowFromAssignment
{
    public static Dictionary<string, double> SummarizeWeightedTrainingDueAssignmentsForWindow(List<WeightedTrainingDueAssignment> items)
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

    public static List<CertificationWindow> DeriveCertificationWindowsFromAssignment(List<WeightedTrainingDueAssignment> items)
    {
        var outValue = new List<CertificationWindow>();
        foreach (var item in items)
        {
            var derived = CertificationWindow.NewCertificationWindow(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveCertificationWindow(CertificationWindow item)
    {
        return WeightedTrainingDueAssignmentPolicy.WeightedTrainingDueAssignmentBucket(item.NetVolume());
    }
}

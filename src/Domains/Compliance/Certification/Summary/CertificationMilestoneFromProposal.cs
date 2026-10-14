namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>Functions of certification milestone from proposal.</summary>
public static class CertificationMilestoneFromProposal
{
    public static Dictionary<string, double> SummarizeExceptionProposalsForMilestone(List<ExceptionProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<CertificationMilestone> DeriveCertificationMilestonesFromProposal(List<ExceptionProposal> items)
    {
        var outValue = new List<CertificationMilestone>();
        foreach (var item in items)
        {
            var derived = CertificationMilestone.NewCertificationMilestone(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignCertificationMilestone(CertificationMilestone item)
    {
        return ExceptionOverridePolicy.NormalizeExceptionOverrideCode(item.Code);
    }
}

namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Functions of certification snapshot from proposal.</summary>
public static class CertificationSnapshotFromProposal
{
    public static Dictionary<string, double> SummarizeAdaptiveCertificationProposalsForSnapshot(List<AdaptiveCertificationProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<CertificationSnapshot> DeriveCertificationSnapshotsFromProposal(List<AdaptiveCertificationProposal> items)
    {
        var outValue = new List<CertificationSnapshot>();
        foreach (var item in items)
        {
            var derived = CertificationSnapshot.NewCertificationSnapshot(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeCertificationSnapshot(CertificationSnapshot item)
    {
        return CertificationAssignmentPolicy.NormalizeCertificationAssignmentCode(item.Code);
    }
}

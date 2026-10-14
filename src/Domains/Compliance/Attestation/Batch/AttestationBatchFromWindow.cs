namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Compliance.Certification.Window;

/// <summary>Functions of attestation batch from window.</summary>
public static class AttestationBatchFromWindow
{
    public static Dictionary<string, double> SummarizeCertificationWindowsForBatch(List<CertificationWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<AttestationBatch> DeriveAttestationBatchesFromWindow(List<CertificationWindow> items)
    {
        var outValue = new List<AttestationBatch>();
        foreach (var item in items)
        {
            var derived = AttestationBatch.NewAttestationBatch(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanAttestationBatch(AttestationBatch item)
    {
        return CertificationGroupPolicy.ClassifyCertificationGroup(item.NetValue());
    }
}

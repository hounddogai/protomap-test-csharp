namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Functions of certification cycle from cycle.</summary>
public static class CertificationCycleFromCycle
{
    public static Dictionary<string, double> SummarizePendingAttestationCyclesForCycle(List<PendingAttestationCycle> items)
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

    public static List<CertificationCycle> DeriveCertificationCyclesFromCycle(List<PendingAttestationCycle> items)
    {
        var outValue = new List<CertificationCycle>();
        foreach (var item in items)
        {
            var derived = CertificationCycle.NewCertificationCycle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveCertificationCycle(CertificationCycle item)
    {
        return AttestationProfilePolicy.NormalizeAttestationProfileCode(item.Code);
    }
}

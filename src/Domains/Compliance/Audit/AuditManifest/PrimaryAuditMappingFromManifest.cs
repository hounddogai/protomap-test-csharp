namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Functions of primary audit mapping from manifest.</summary>
public static class PrimaryAuditMappingFromManifest
{
    public static Dictionary<string, double> SummarizeManualExceptionManifestsForMapping(List<ManualExceptionManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<PrimaryAuditMapping> DerivePrimaryAuditMappingsFromManifest(List<ManualExceptionManifest> items)
    {
        var outValue = new List<PrimaryAuditMapping>();
        foreach (var item in items)
        {
            var derived = PrimaryAuditMapping.NewPrimaryAuditMapping(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputePrimaryAuditMapping(PrimaryAuditMapping item)
    {
        return DefaultExceptionAllocationPolicy.DefaultExceptionAllocationWeight(1, true);
    }
}

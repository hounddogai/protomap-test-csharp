namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Functions of lenient license seat audit from override.</summary>
public static class LenientLicenseSeatAuditFromOverride
{
    public static Dictionary<string, double> SummarizeWarrantyOverridesForAudit(List<WarrantyOverride> items)
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

    public static List<LenientLicenseSeatAudit> DeriveLenientLicenseSeatAuditsFromOverride(List<WarrantyOverride> items)
    {
        var outValue = new List<LenientLicenseSeatAudit>();
        foreach (var item in items)
        {
            var derived = LenientLicenseSeatAudit.NewLenientLicenseSeatAudit(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLenientLicenseSeatAudit(LenientLicenseSeatAudit item)
    {
        return WarrantyOverridePolicy.WarrantyOverrideBucket(item.AdjustedLoad());
    }
}

namespace Acme.Workforce.Compliance.Exception.Proposal;

using Acme.Workforce.Compliance.Audit.Mapping;

/// <summary>Functions of exception override from template.</summary>
public static class ExceptionOverrideFromTemplate
{
    public static Dictionary<string, double> SummarizeAuditTemplatesForOverride(List<AuditTemplate> items)
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

    public static List<ExceptionOverride> DeriveExceptionOverridesFromTemplate(List<AuditTemplate> items)
    {
        var outValue = new List<ExceptionOverride>();
        foreach (var item in items)
        {
            var derived = ExceptionOverride.NewExceptionOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeExceptionOverride(ExceptionOverride item)
    {
        return AuditMappingPolicy.NormalizeAuditMappingCode(item.Code);
    }
}

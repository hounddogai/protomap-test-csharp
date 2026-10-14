namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.LeaveRequest.Forecast;

/// <summary>Functions of primary return plan manifest from template.</summary>
public static class PrimaryReturnPlanManifestFromTemplate
{
    public static Dictionary<string, double> SummarizeFallbackLeaveRequestTemplatesForManifest(List<FallbackLeaveRequestTemplate> items)
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

    public static List<PrimaryReturnPlanManifest> DerivePrimaryReturnPlanManifestsFromTemplate(List<FallbackLeaveRequestTemplate> items)
    {
        var outValue = new List<PrimaryReturnPlanManifest>();
        foreach (var item in items)
        {
            var derived = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePrimaryReturnPlanManifest(PrimaryReturnPlanManifest item)
    {
        return FallbackLeaveRequestTemplatePolicy.NormalizeFallbackLeaveRequestTemplateCode(item.Code);
    }
}

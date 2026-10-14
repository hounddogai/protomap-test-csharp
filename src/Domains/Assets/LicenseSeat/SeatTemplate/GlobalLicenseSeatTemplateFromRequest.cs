namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.Refresh.RefreshRequest;

/// <summary>Functions of global license seat template from request.</summary>
public static class GlobalLicenseSeatTemplateFromRequest
{
    public static Dictionary<string, double> SummarizeLenientRefreshRequestsForTemplate(List<LenientRefreshRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<GlobalLicenseSeatTemplate> DeriveGlobalLicenseSeatTemplatesFromRequest(List<LenientRefreshRequest> items)
    {
        var outValue = new List<GlobalLicenseSeatTemplate>();
        foreach (var item in items)
        {
            var derived = GlobalLicenseSeatTemplate.NewGlobalLicenseSeatTemplate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignGlobalLicenseSeatTemplate(GlobalLicenseSeatTemplate item)
    {
        return GlobalRefreshOverridePolicy.GlobalRefreshOverrideWeight(2, true);
    }
}

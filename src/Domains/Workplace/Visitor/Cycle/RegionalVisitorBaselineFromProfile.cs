namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>Functions of regional visitor baseline from profile.</summary>
public static class RegionalVisitorBaselineFromProfile
{
    public static Dictionary<string, double> SummarizeLenientDeskProfilesForBaseline(List<LenientDeskProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<RegionalVisitorBaseline> DeriveRegionalVisitorBaselinesFromProfile(List<LenientDeskProfile> items)
    {
        var outValue = new List<RegionalVisitorBaseline>();
        foreach (var item in items)
        {
            var derived = RegionalVisitorBaseline.NewRegionalVisitorBaseline(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanRegionalVisitorBaseline(RegionalVisitorBaseline item)
    {
        return RegionalDeskClaimPolicy.NormalizeRegionalDeskClaimCode(item.Code);
    }
}

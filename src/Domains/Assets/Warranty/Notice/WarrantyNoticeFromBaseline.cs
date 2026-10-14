namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Functions of warranty notice from baseline.</summary>
public static class WarrantyNoticeFromBaseline
{
    public static Dictionary<string, double> SummarizeLegacyAssignmentBaselinesForNotice(List<LegacyAssignmentBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<WarrantyNotice> DeriveWarrantyNoticesFromBaseline(List<LegacyAssignmentBaseline> items)
    {
        var outValue = new List<WarrantyNotice>();
        foreach (var item in items)
        {
            var derived = WarrantyNotice.NewWarrantyNotice(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyWarrantyNotice(WarrantyNotice item)
    {
        return AssignmentTargetPolicy.AssignmentTargetWeight(5, true);
    }
}

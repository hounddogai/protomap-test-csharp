namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional merit cycle allocation.</summary>
public sealed record RegionalMeritCycleAllocationView(string Code, string NameText, string Revision, string Threshold, string Amount, string CreatedAt, string CostCenterText, string Maximum, string ExpiresAt, string StatusLabel)
{
    public static RegionalMeritCycleAllocationView ToRegionalMeritCycleAllocationView(RegionalMeritCycleAllocation item)
    {
        return new RegionalMeritCycleAllocationView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalMeritCycleAllocationView> ToRegionalMeritCycleAllocationViews(List<RegionalMeritCycleAllocation> items)
    {
        var views = new List<RegionalMeritCycleAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalMeritCycleAllocationView(item));
        }
        return views;
    }

    public static double SumRegionalMeritCycleAllocationView(RegionalMeritCycleAllocationView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedThreshold + parsedAmount;
    }
}

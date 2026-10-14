namespace Acme.Workforce.Workplace.Desk.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk tier.</summary>
public sealed record DeskTierView(string CodeText, string NameText, string FloorValue, string CreatedAt, string ThresholdText, string TeamText, string JobCodeText, string CostCenterText, string Description, string StatusLabel)
{
    public static DeskTierView ToDeskTierView(DeskTier item)
    {
        return new DeskTierView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.JobCode,
            item.CostCenter,
            Text.Truncate(item.Description, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskTierView> ToDeskTierViews(List<DeskTier> items)
    {
        var views = new List<DeskTierView>();
        foreach (var item in items)
        {
            views.Add(ToDeskTierView(item));
        }
        return views;
    }

    public static double SumDeskTierView(DeskTierView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedCreatedAt + parsedThreshold;
    }
}

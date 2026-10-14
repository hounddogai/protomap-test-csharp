namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan quota.</summary>
public sealed record FloorPlanQuotaView(string Code, string Name, string Sequence, string TenureMonths, string CurrencyText, string Grade, string ManagerId, string StatusLabel)
{
    public static FloorPlanQuotaView ToFloorPlanQuotaView(FloorPlanQuota item)
    {
        return new FloorPlanQuotaView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            Text.Truncate(item.Grade, 120),
            Text.Truncate(item.ManagerId, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanQuotaView> ToFloorPlanQuotaViews(List<FloorPlanQuota> items)
    {
        var views = new List<FloorPlanQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanQuotaView(item));
        }
        return views;
    }

    public static double SumFloorPlanQuotaView(FloorPlanQuotaView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedTenureMonths;
    }
}

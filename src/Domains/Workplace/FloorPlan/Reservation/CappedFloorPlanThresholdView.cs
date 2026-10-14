namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped floor plan threshold.</summary>
public sealed record CappedFloorPlanThresholdView(string Code, string Label, string Cost, string Phone, string AttemptsText, string Visible, string OwnerIdText, string Segment, string Maximum, string SourceText, string TenureMonths, string StatusLabel)
{
    public static CappedFloorPlanThresholdView ToCappedFloorPlanThresholdView(CappedFloorPlanThreshold item)
    {
        return new CappedFloorPlanThresholdView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.OwnerId, 40),
            item.Segment,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedFloorPlanThresholdView> ToCappedFloorPlanThresholdViews(List<CappedFloorPlanThreshold> items)
    {
        var views = new List<CappedFloorPlanThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToCappedFloorPlanThresholdView(item));
        }
        return views;
    }

    public static double SumCappedFloorPlanThresholdView(CappedFloorPlanThresholdView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedAttempts + parsedMaximum;
    }
}

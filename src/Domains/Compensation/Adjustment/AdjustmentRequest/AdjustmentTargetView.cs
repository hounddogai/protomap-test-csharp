namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using System.Globalization;

/// <summary>How screens and exports show a adjustment target.</summary>
public sealed record AdjustmentTargetView(string CodeText, string Label, string Department, string CreatedAt, string RegionText, string Capacity, string DurationMinutesText, string Notes, string Phone, string Ceiling, string Archived, string StatusLabel)
{
    public static AdjustmentTargetView ToAdjustmentTargetView(AdjustmentTarget item)
    {
        return new AdjustmentTargetView(
            item.Code,
            item.Label,
            item.Department,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Phone,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentTargetView> ToAdjustmentTargetViews(List<AdjustmentTarget> items)
    {
        var views = new List<AdjustmentTargetView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentTargetView(item));
        }
        return views;
    }

    public static double SumAdjustmentTargetView(AdjustmentTargetView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedCapacity + (double)parsedDurationMinutes;
    }
}

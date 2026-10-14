namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using System.Globalization;

/// <summary>How screens and exports show a weighted merit cycle segment.</summary>
public sealed record WeightedMeritCycleSegmentView(string CodeText, string TitleText, string Threshold, string FloorValueText, string Region, string WindowDays, string BatchSizeText, string OwnerId, string PhoneText, string StatusLabel)
{
    public static WeightedMeritCycleSegmentView ToWeightedMeritCycleSegmentView(WeightedMeritCycleSegment item)
    {
        return new WeightedMeritCycleSegmentView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedMeritCycleSegmentView> ToWeightedMeritCycleSegmentViews(List<WeightedMeritCycleSegment> items)
    {
        var views = new List<WeightedMeritCycleSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedMeritCycleSegmentView(item));
        }
        return views;
    }

    public static double SumWeightedMeritCycleSegmentView(WeightedMeritCycleSegmentView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedFloorValue + (double)parsedWindowDays;
    }
}

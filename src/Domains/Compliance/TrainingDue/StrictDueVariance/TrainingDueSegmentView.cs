namespace Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a training due segment.</summary>
public sealed record TrainingDueSegmentView(string Code, string Name, string Country, string FloorValue, string WindowDays, string Discount, string Segment, string PriorityText, string VisibleText, string StatusLabel)
{
    public static TrainingDueSegmentView ToTrainingDueSegmentView(TrainingDueSegment item)
    {
        return new TrainingDueSegmentView(
            item.Code,
            item.Name,
            Text.Truncate(item.Country, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TrainingDueSegmentView> ToTrainingDueSegmentViews(List<TrainingDueSegment> items)
    {
        var views = new List<TrainingDueSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToTrainingDueSegmentView(item));
        }
        return views;
    }

    public static double SumTrainingDueSegmentView(TrainingDueSegmentView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedWindowDays + parsedDiscount;
    }
}

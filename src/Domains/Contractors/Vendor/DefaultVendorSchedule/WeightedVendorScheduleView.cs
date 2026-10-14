namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

using System.Globalization;

/// <summary>How screens and exports show a weighted vendor schedule.</summary>
public sealed record WeightedVendorScheduleView(string CodeText, string Name, string Weight, string BatchSize, string ScoreText, string VisibleText, string EmployeeId, string StatusLabel)
{
    public static WeightedVendorScheduleView ToWeightedVendorScheduleView(WeightedVendorSchedule item)
    {
        return new WeightedVendorScheduleView(
            item.Code,
            item.Name,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.EmployeeId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedVendorScheduleView> ToWeightedVendorScheduleViews(List<WeightedVendorSchedule> items)
    {
        var views = new List<WeightedVendorScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedVendorScheduleView(item));
        }
        return views;
    }

    public static double SumWeightedVendorScheduleView(WeightedVendorScheduleView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedBatchSize + parsedScore;
    }
}
